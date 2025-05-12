#include "ResponseReader.h"

const char *prefixHttp = "HTTP";
const char *prefixContentLength = "Content-Length:";

const int MAX_CONTENT_LENGTH = 8192;

char asciiLower(char c) {
  if ((c >= 'A') && (c <= 'Z')) {
    return c + 32;
  } else {
    return c;
  }
}

ResponseReader::ResponseReader(Buffer *contentReceiver) {
  _isParsedSuccessfully = false;
  _statusCode = 0;
  _contentReceiver = contentReceiver;

  _state = 0;
  _comparerPosition = 0;
  _canBeContentLength = false;
  _wasContentLength = false;
  _contentLength = 0;
  _readContentBytes = 0;
}

void ResponseReader::nextByte(char c) {
  switch (_state) {
    case 0: // read "HTTP"
      if (c == prefixHttp[_comparerPosition]) {
        // All good, advance
        _comparerPosition++;
        if (prefixHttp[_comparerPosition] == 0) {
          // Whole prefix recognized, next state
          _state = 1;
        }
      } else {
        // Send to error state (no return)
        _state = 99;
      }
      break;

    case 1: // after "HTTP" waiting for space
      if (c == ' ') {
        _state = 2;
      } else if ((c >= 0) && (c < ' ')) {
        // Other whitespace character, control character - error
        _state = 99;
      }
      break;

    case 2: // after "HTTP/1.1 " waiting for number
      if (c == ' ') {
        // Continue in current state
      } else if ((c >= '0') && (c <= '9')) {
        _statusCode = c - '0';
        _state = 3;
      } else {
        // Anything else is error
        _state = 99;
      }
      break;

    case 3: // assemblying status code
      if ((c >= '0') && (c <= '9')) {
        // Simple overflow check
        if (_statusCode >= 3000) {
          _state = 99;
        } else {
          _statusCode *= 10;
          _statusCode += (c - '0');
        }
      } else if (c == ' ') {
        // From now on the response from server is considered "correct" unless we encounter some garbage later.
        _isParsedSuccessfully = true;
        // Wait till the end of line
        _state = 4;
      } else if (c == 13) {
        // The string must follow after code, but ok.
        _isParsedSuccessfully = true;
        _state = 5;
      } else if (c == 10) {
        // The string must follow after code, but ok.
        _isParsedSuccessfully = true;
        _state = 6;
      } else {
        // There should be a space or some sort of delimiter after a status code
        _state = 99;
      }
      break;

    case 4: // In header: go till the end of current line
      if (c == 13) {
        _state = 5;
      } else if (c == 10) {
        _state = 6;
      }
      break;

    case 5: // after #13 while going till the end of header's line
      if (c == 13) {
        // stay
      } else if (c == 10) {
        _state = 6;
      } else {
        _state = 4;
      }
      break;

    case 6: // expecting new line of header to begin
      if (c == 13) {
        // End of header?
        _state = 7;
      } else if (c == 10) {
        // End of header.
        if (_contentLength > 0) {
          // Prepare to read content
          _readContentBytes = 0;
          if (_contentReceiver) {
            _contentReceiver->seek(0);
            _contentReceiver->setLength(0);
          }
          _state = 8;
        } else {
          // We allow not mentioned content length or content length 0.
          // Final "success" state with no exit out of it:
          _state = 100;
        }
      } else {
        // Next line of header starts. Start recognizing.
        _comparerPosition = 0;
        _canBeContentLength = true;
        _state = 9;

        // Here is like first iteration of state 9 already:
        if (asciiLower(c) != asciiLower(prefixContentLength[_comparerPosition])) {
          _canBeContentLength = false;
        }
        _comparerPosition++;
        // None of recognized strings is shorter than 2 symbols, so we're good.
      }
      break;

    case 7: // #13 at the beginning of header line
      if (c == 10) {
        // End of header. Copypaste from state 6:
        if (_contentLength > 0) {
          _readContentBytes = 0;
          if (_contentReceiver) {
            _contentReceiver->seek(0);
            _contentReceiver->setLength(0);
          }
          _state = 8;
        } else {
          _state = 100;
        }
      } else {
        // Special character where text is supposed to be? What?
        _isParsedSuccessfully = false;
        _state = 99;
      }
      break;

    case 8: // Reading content
      if (_contentReceiver) {
        _contentReceiver->writeChar(c);
      }
      _readContentBytes++;
      if (_readContentBytes == _contentLength) {
        // Now everything is done, any bytes further don't matter.
        _isParsedSuccessfully = true;
        _state = 100;
      }
      break;

    case 9: // Trying to recognize current header
      if (c == 13) {
        // Whether it is new line or rogue #13 - no chance of recognizing any known headers anymore.
        // Switch to "just wait until it ends" state:
        _state = 5;
      } else if (c == 10) {
        _state = 6;
      } else {
        // Trying to match any of known sumstrings (currently 1)
        if (_canBeContentLength) {
          if (asciiLower(c) != asciiLower(prefixContentLength[_comparerPosition])) {
            _canBeContentLength = false;
          }
        }
        _comparerPosition++;
        if (_canBeContentLength) {
          if (prefixContentLength[_comparerPosition] == 0) {
            // Content length that is!
            _canBeContentLength = false; // just ensure against reading out of range in future

            // Now if we stop until value or the content itself is read - it's error.
            _isParsedSuccessfully = false;

            if (_wasContentLength) {
              // Duplication not allowed
              _state = 99;
            } else {
              _wasContentLength = true;
              _state = 10;
            }
          }
        }
      }
      break;

    case 10: // "Content-Length": Wait through spaces until content length starts
      if (c == ' ') {
        // stay
      } else if ((c >= '0') && (c <= '9')) {
        _contentLength = (c - '0');
        _state = 11;
      } else {
        _state = 99;
      }
      break;

    case 11: // Assemblying content length
      if ((c >= '0') && (c <= '9')) {
        int digit = c - '0';
        if ((_contentLength > 3276) || ((_contentLength == 3276) && (digit > 7))) {
          // Int overflow
          _state = 99;
        } else {
          _contentLength *= 10;
          _contentLength += digit;
          if (_contentLength > MAX_CONTENT_LENGTH) {
            _state = 99;
          }
          // else continue
        }
      } else if (c == 13) {
        // Success. Go next header's line.
        _state = 12;
      } else if (c == 10) {
        // Success. Go next header's line.
        // If no content then stopping in current state is legal:
        _isParsedSuccessfully = (_contentLength == 0);
        _state = 6;
      } else if (c == ' ') {
        // Success. Ignore any character till the end of line
        // If no content then stopping in current state is legal:
        _isParsedSuccessfully = (_contentLength == 0);
        _state = 4;
      } else {
        _state = 99;
      }
      break;

    case 12: // #13 after content length
      if (c == 10) {
        // If no content then stopping in current state is legal:
        _isParsedSuccessfully = (_contentLength == 0);
        _state = 6;
      } else {
        _isParsedSuccessfully = false;
        _state = 99;
      }
      break;
  }
}

bool ResponseReader::isParsedSuccessfully() {
  return _isParsedSuccessfully;
}

int ResponseReader::getStatusCode() {
  return _statusCode;
}