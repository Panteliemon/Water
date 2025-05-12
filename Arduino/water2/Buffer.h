#pragma once

// Dynamically growing array of bytes.
class Buffer {
  private:
    char *_p;
    int _allocatedLength;
    int _usedLength;
    int _pos;
  public:
    Buffer();
    ~Buffer();

    // Pointer to the beginning of the buffer
    char *p();

    int getLength();
    void setLength(int value);

    // R/W position. Can point directly to the end of buffer, but cannot be less than 0 or greater than getLength().
    int getPos();
    int seek(int pos);

    void writeChar(char c);
    void writeDecimalInt(int value);
    void writeDecimalLong(long value);

    // If contents of the buffer don't finish with \0 char, adds it to the end.
    // Doesn't change the R/W position!
    void ensureTerminalZero();

    // In case of success advances position by 1.
    bool tryReadChar(char &c);
    // Position before call should be before the first digit.
    // Position after the call (if success) is either before the first non-digit or at the end of the buffer (whatever is earlier)
    bool tryReadDecimalInt(int &value);
    // Position before call should be before the first digit.
    // Position after the call (if success) is either before the first non-digit or at the end of the buffer (whatever is earlier)
    bool tryReadDecimalLong(long &value);
};