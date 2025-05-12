#pragma once

#include "Buffer.h"

class ResponseReader {
  private:
    bool _isParsedSuccessfully;
    int _statusCode;
    Buffer *_contentReceiver;

    // States for parsing
    int _state;
    int _comparerPosition;
    bool _canBeContentLength;
    bool _wasContentLength;
    int _contentLength;
    int _readContentBytes;
  public:
    ResponseReader(Buffer *contentReceiver);

    void nextByte(char c);

    bool isParsedSuccessfully();
    int getStatusCode();
};