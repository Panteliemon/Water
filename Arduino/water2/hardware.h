#pragma once

void initHardware();

// ---------- Inputs ----------
bool getEStop();
bool getExtA();
bool getExtB();
bool getExtX();

typedef enum {
  I_ESTOP = 1,
  I_EXTA = 2,
  I_EXTB = 4,
  I_EXTX = 8,
} InputMask;

inline InputMask operator|(InputMask x, InputMask y) { return static_cast<InputMask>(static_cast<int>(x) | static_cast<int>(y)); }
inline InputMask operator&(InputMask x, InputMask y) { return static_cast<InputMask>(static_cast<int>(x) & static_cast<int>(y)); }
inline InputMask &operator|=(InputMask &x, InputMask y) {
  x = x | y;
  return x;
}

// Waits until any of specified inputs changes its state to true. If already true - exits immediately. Result - mask of inputs which have changed.
InputMask waitForSet(InputMask inputsToWait);
// Waits until any of specified inputs changes its state to false. If already false - exits immediately. Result - mask of inputs which have changed.
InputMask waitForReset(InputMask inputsToWait);

// ---------- Outputs ----------

// enableValve automatically disables all the other valves
void enableValve(int valveIndex);
void disableValves();

void enableMotor();
void disableMotor();

extern volatile unsigned int wCounter;

void setExtLed(bool value);