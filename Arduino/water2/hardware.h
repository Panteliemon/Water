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

inline InputMask operator&(InputMask x, InputMask y) { return static_cast<InputMask>(static_cast<int>(x) & static_cast<int>(y)); }
inline InputMask operator|(InputMask x, InputMask y) { return static_cast<InputMask>(static_cast<int>(x) | static_cast<int>(y)); }
inline InputMask operator~(InputMask x) { return static_cast<InputMask>(!static_cast<int>(x)); }
inline InputMask operator^(InputMask x, InputMask y) { return static_cast<InputMask>(static_cast<int>(x) ^ static_cast<int>(y)); }
inline InputMask &operator|=(InputMask &x, InputMask y) {
  x = x | y;
  return x;
}

// Waits until any of specified inputs changes its state to true. If already true - exits immediately. Result - mask of inputs which have changed.
InputMask waitForSet(InputMask inputsToWait);
// Waits until any of specified inputs changes its state to false. If already false - exits immediately. Result - mask of inputs which have changed.
InputMask waitForReset(InputMask inputsToWait);

struct InputsChange {
  InputMask changedInputs;
  InputMask inputsState;

  bool isChanged(InputMask inputs) { return (inputs & changedInputs) != 0; }
  bool isOn(InputMask inputs) { return (inputs & inputsState) != 0; }
  bool turnedOn(InputMask inputs) { return (inputs & inputsState & changedInputs) != 0; }
  bool turnedOff(InputMask inputs) { return (inputs & (~inputsState) & changedInputs) != 0; }
};
// Waits until any of specified inputs changes its state compared to right now (when the func was just called)
InputsChange waitForChange(InputMask inputsToWait);

// ---------- Outputs ----------

// enableValve automatically disables all the other valves
void enableValve(int valveIndex);
void disableValves();

void enableMotor();
void disableMotor();

extern volatile unsigned int wCounter;

void setExtLed(bool value);