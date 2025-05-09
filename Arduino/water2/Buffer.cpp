#include "Serial.h"
#include "Buffer.h"

const int DEFAULT_ALLOCATED_LENGTH = 32;

Buffer::Buffer() {
  _p = new char[DEFAULT_ALLOCATED_LENGTH];
  _allocatedLength = DEFAULT_ALLOCATED_LENGTH;
  _usedLength = 0;
  _pos = 0;
}

Buffer::~Buffer() {
  delete _p;
  _p = 0;
}

char *Buffer::p() {
  return _p;
}

int Buffer::getLength() {
  return _usedLength;
}

void Buffer::setLength(int value) {
  if (value < 0)
    return;

  if (value > _allocatedLength) {
    int newAllocatedLength = _allocatedLength << 1;
    while (value > _allocatedLength) {
      newAllocatedLength <<= 1;
    }

    char *newP = new char[newAllocatedLength];
    for (int i=0; i<_usedLength; i++)
      newP[i] = _p[i];
    
    delete _p;
    _p = newP;
    _allocatedLength = newAllocatedLength;
  }

  _usedLength = value;
  if (_pos > _usedLength)
    _pos = _usedLength;
}

int Buffer::getPos() {
  return _pos;
}

int Buffer::seek(int pos) {
  if (pos < 0)
    _pos = 0;
  else if (pos > _usedLength)
    _pos = _usedLength;
  else
    _pos = pos;
}

void Buffer::writeChar(char c) {
  if (_pos == _usedLength) {
    setLength(_usedLength + 1);
  }

  _p[_pos] = c;
  _pos++;
}

void Buffer::writeDecimalInt(int value) {
  if (value < 0) {
    writeChar('-');
    if (value == -32768) {
      Serial.println("Ej tu dirst!");
      writeChar('3'); writeChar('2'); writeChar('7'); writeChar('6'); writeChar('8');
    } else {
      writeDecimalInt(-value);
    }
  } else if (value > 0) {
    // Compare with 10th of the value in order to avoid overflow
    int valueDividedBy10 = value / 10;
    int currentPowerOf10 = 1;
    while (currentPowerOf10 <= valueDividedBy10) {
      currentPowerOf10 *= 10;
    }

    while (currentPowerOf10 > 0) {
      int digit = (value / currentPowerOf10) % 10;
      writeChar('0' + digit);
      currentPowerOf10 /= 10;
    }
  } else {
    writeChar('0');
  }
}

void Buffer::writeDecimalLong(long value) {
  if (value < 0) {
    writeChar('-');
    if (value == -2147483648) {
      Serial.println("Ej tu nost!");
      writeChar('2'); writeChar('1'); writeChar('4'); writeChar('7');
      writeChar('4'); writeChar('8'); writeChar('3');
      writeChar('6'); writeChar('4'); writeChar('8');
    } else {
      writeDecimalLong(-value);
    }
  } else if (value > 0) {
    // Compare with 10th of the value in order to avoid overflow
    long valueDividedBy10 = value / (long)10;
    long currentPowerOf10 = 1;
    while (currentPowerOf10 <= valueDividedBy10) {
      currentPowerOf10 *= 10;
    }

    while (currentPowerOf10 > 0) {
      long digit = (value / currentPowerOf10) % 10;
      writeChar('0' + digit);
      currentPowerOf10 /= 10;
    }
  } else {
    writeChar('0');
  }
}

void Buffer::ensureTerminalZero() {
  if (_usedLength == 0) {
    setLength(1);
    _p[0] = 0;
  } else {
    if (_p[_usedLength - 1] != 0) {
      setLength(_usedLength + 1);
      _p[_usedLength - 1] = 0; // achtung: new _usedLength!
    }
  }
}

bool Buffer::tryReadChar(char &c) {
  if (_pos < _usedLength) {
    c = _p[_pos];
    _pos++;
    return true;
  } else {
    return false;
  }
}

bool Buffer::tryReadDecimalInt(int &value) {
  char c = 0;
  if (tryReadChar(c)) {
    bool isNegative = false;
    if (c == '-') {
      isNegative = true;
      if (!tryReadChar(c)) {
        return false;
      }
    }
    
    // At least one digit is required
    if ((c >= '0') && (c <= '9')) {
      int result = c - '0';

      while (tryReadChar(c)) {
        if ((c >= '0') && (c <= '9')) {
          int digit = c - '0';
          if (result > 3276) {
            // No more digits allowed: overflow
            return false;
          } else if (result == 3276) {
            if (isNegative) {
              if (digit > 8) {
                return false;
              } else if (digit == 8) {
                // Not overflow when negative, but we cannot write such value into result since result is positive.
                // Then handle personally! Don't use outer cycle anymore.
                // Next symbol must be not a digit to succeed
                if (!tryReadChar(c)) {
                  value = -32768;
                  return true;
                }
                if ((c >= '0') && (c <= '9')) {
                  // Overflow: a digit after -32768
                  return false;
                } else {
                  _pos--; // cursor must stop before the non-digit
                  value = -32768;
                  return true;
                }
              }
            } else {
              if (digit > 7) {
                return false;
              }
            }
            // Not an overflow this time, can multiply.
          }

          // Not an overflow
          result *= 10;
          result += digit;
        } else {
          // Number ended. Move cursor back so it's before the symbol we've just read
          _pos--;
          break;
        }
      }

      value = (isNegative) ? -result : result;
      return true;
    } else {
      return false;
    }
  }

  return false;
}

bool Buffer::tryReadDecimalLong(long &value) {
  char c = 0;
  if (tryReadChar(c)) {
    bool isNegative = false;
    if (c == '-') {
      isNegative = true;
      if (!tryReadChar(c)) {
        return false;
      }
    }
    
    // At least one digit is required
    if ((c >= '0') && (c <= '9')) {
      long result = c - '0';

      while (tryReadChar(c)) {
        if ((c >= '0') && (c <= '9')) {
          long digit = c - '0';
          if (result > 214748364) {
            // No more digits allowed: overflow
            return false;
          } else if (result == 214748364) {
            if (isNegative) {
              if (digit > 8) {
                return false;
              } else if (digit == 8) {
                // Not overflow when negative, but we cannot write such value into result since result is positive.
                // Then handle personally! Don't use outer cycle anymore.
                // Next symbol must be not a digit to succeed
                if (!tryReadChar(c)) {
                  value = -2147483648;
                  return true;
                }
                if ((c >= '0') && (c <= '9')) {
                  // Overflow: a digit after -2147483648
                  return false;
                } else {
                  _pos--; // cursor must stop before the non-digit
                  value = -2147483648;
                  return true;
                }
              }
            } else {
              if (digit > 7) {
                return false;
              }
            }
            // Not an overflow this time, can multiply.
          }

          // Not an overflow
          result *= (long)10;
          result += digit;
        } else {
          // Number ended. Move cursor back so it's before the symbol we've just read
          _pos--;
          break;
        }
      }

      value = (isNegative) ? -result : result;
      return true;
    } else {
      return false;
    }
  }

  return false;
}