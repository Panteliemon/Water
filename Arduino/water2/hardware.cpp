#include <Arduino.h>
#include "hardware.h"

// Flow meter counter signal normalized to 5V inverted
const uint8_t PIN_CNT = 2;
// State of ESTOP inverted
const uint8_t PIN_EST = 3;

// Shift register clock
const uint8_t PIN_CL = 4;
// Shift register reset
const uint8_t PIN_RST = 5;
// Shift register data
const uint8_t PIN_DAT = 6;

// Enable valves
const uint8_t PIN_ENV = 8;
// Enable motor
const uint8_t PIN_ENM = 9;

// Extension button A
const uint8_t PIN_EXT_A = 10;
// Extension button B
const uint8_t PIN_EXT_B = 11;
// Extension button X
const uint8_t PIN_EXT_X = 12;
// Extension LED
const uint8_t PIN_EXT_LED = 13;

volatile unsigned int wCounter;

void onWCounter() {
  wCounter++;
}

void clockPulse() {
  digitalWrite(PIN_CL, HIGH);
  digitalWrite(PIN_CL, LOW);
}

void initHardware() {
  pinMode(PIN_CNT, INPUT);
  pinMode(PIN_EST, INPUT);
  pinMode(PIN_CL, OUTPUT);
  pinMode(PIN_RST, OUTPUT);
  pinMode(PIN_DAT, OUTPUT);
  pinMode(PIN_ENV, OUTPUT);
  pinMode(PIN_ENM, OUTPUT);
  pinMode(PIN_EXT_A, INPUT);
  pinMode(PIN_EXT_B, INPUT);
  pinMode(PIN_EXT_X, INPUT);
  pinMode(PIN_EXT_LED, OUTPUT);

  digitalWrite(PIN_CL, LOW);
  digitalWrite(PIN_RST, LOW);
  digitalWrite(PIN_DAT, LOW);
  digitalWrite(PIN_ENV, LOW);
  digitalWrite(PIN_ENM, LOW);
  digitalWrite(PIN_EXT_LED, LOW);

  attachInterrupt(digitalPinToInterrupt(PIN_CNT), &onWCounter, FALLING);
}

bool getEStop() {
  return digitalRead(PIN_EST) == LOW;
}

bool getExtA() {
  return digitalRead(PIN_EXT_A) == HIGH;
}

bool getExtB() {
  return digitalRead(PIN_EXT_B) == HIGH;
}

bool getExtX() {
  return digitalRead(PIN_EXT_X) == HIGH;
}

InputMask waitForSet(InputMask inputsToWait) {
  if (inputsToWait == 0) {
    return (InputMask)0;
  }

  while (true) {
    InputMask switchedNowMask = (InputMask)0;

    if ((inputsToWait & I_ESTOP) != 0) {
      if (getEStop()) {
        switchedNowMask |= I_ESTOP;
      }
    }

    if ((inputsToWait & I_EXTA) != 0) {
      if (getExtA()) {
        switchedNowMask |= I_EXTA;
      }
    }

    if ((inputsToWait & I_EXTB) != 0) {
      if (getExtB()) {
        switchedNowMask |= I_EXTB;
      }
    }

    if ((inputsToWait & I_EXTX) != 0) {
      if (getExtX()) {
        switchedNowMask |= I_EXTX;
      }
    }

    if (switchedNowMask != 0) {
      return switchedNowMask;
    }
  }
}

InputMask waitForReset(InputMask inputsToWait) {
  if (inputsToWait == 0) {
    return (InputMask)0;
  }

  while (true) {
    InputMask switchedNowMask = (InputMask)0;

    if ((inputsToWait & I_ESTOP) != 0) {
      if (!getEStop()) {
        switchedNowMask |= I_ESTOP;
      }
    }

    if ((inputsToWait & I_EXTA) != 0) {
      if (!getExtA()) {
        switchedNowMask |= I_EXTA;
      }
    }

    if ((inputsToWait & I_EXTB) != 0) {
      if (!getExtB()) {
        switchedNowMask |= I_EXTB;
      }
    }

    if ((inputsToWait & I_EXTX) != 0) {
      if (!getExtX()) {
        switchedNowMask |= I_EXTX;
      }
    }

    if (switchedNowMask != 0) {
      return switchedNowMask;
    }
  }
}

void enableValve(int valveIndex) {
  disableValves();

  if ((valveIndex < 0) || (valveIndex > 7)) {
    return;
  }

  digitalWrite(PIN_RST, HIGH);
  digitalWrite(PIN_DAT, HIGH);
  clockPulse();

  digitalWrite(PIN_DAT, LOW);

  int index = valveIndex;
  while (index > 0) {
    clockPulse();
    index--;
  }

  digitalWrite(PIN_ENV, HIGH);
}

void disableValves() {
  digitalWrite(PIN_ENV, LOW);
  digitalWrite(PIN_RST, LOW);
}

void enableMotor() {
  digitalWrite(PIN_ENM, HIGH);
}

void disableMotor() {
  digitalWrite(PIN_ENM, LOW);
}

void setExtLed(bool value) {
  digitalWrite(PIN_EXT_LED, value ? HIGH : LOW);
}