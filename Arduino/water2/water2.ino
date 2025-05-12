#include "model.h"
#include "networking.h"
#include "hardware.h"
#include "pour.h"

void setup() {
  initHardware();

  Serial.begin(9600);
  while (!Serial);

  // Debounced inputs: give time for level to establish
  delay(200);
}

void loop() {
  // Program selection
  if (getExtA()) {
    if (getExtB()) {
      calibration();
    } else {
      fillTubes();
    }
  } else {
    if (getExtB()) {
      while (true) ;
    } else {
      initNetwork();
      Serial.print("Scaling factor: ");
      Serial.print(getCountsPerLiter());
      Serial.println(" L^-1");
      mainLoop();
    }
  }
}

void mainLoop() {
  while (true) {
    blinkMedium(1);

    Task currentTask;
    if (tryGetNextTask(currentTask)) {
      for (int i=0; i<currentTask.itemsCount; i++) {
        Serial.print(currentTask.id);
        Serial.print(", ");
        Serial.print(i + 1);
        Serial.print("/");
        Serial.print(currentTask.itemsCount);
        Serial.print(": [");
        Serial.print(currentTask.items[i].valveIndex);
        Serial.print("] -> ");
        Serial.print(currentTask.items[i].volumeMl);
        Serial.println(" ml");

        currentTask.items[i].status = pour(currentTask.items[i].valveIndex, ((double)currentTask.items[i].volumeMl) / 1000.0);
        reportTaskResult(currentTask, i);

        delay(2000);
      }
    }

    delay(60000);
  }
}

void blinkFast(int numberOfTimes) {
  for (int i=0; i<numberOfTimes; i++) {
    if (i > 0) {
      delay(170);
    }
    setExtLed(true);
    delay(80);
    setExtLed(false);
  }
}

void blinkMedium(int numberOfTimes) {
  for (int i=0; i<numberOfTimes; i++) {
    if (i > 0) {
      delay(250);
    }
    setExtLed(true);
    delay(250);
    setExtLed(false);
  }
}

void blinkSlow(int numberOfTimes) {
  for (int i=0; i<numberOfTimes; i++) {
    if (i > 0) {
      delay(1000);
    }
    setExtLed(true);
    delay(1000);
    setExtLed(false);
  }
}

// Initiates sequence of user entering valve number:
// - demand for press ESTOP with Ext LED constantly glowing
// - beep-boop
// - display Valve No entered by user
int enterValveIndexUnderEStop() {
  while (true) {
    if (!getEStop()) {
      setExtLed(true);
      waitForSet(I_ESTOP);
      setExtLed(false);
    }

    int xPressedCount = 0;

    while (true) {
      InputsChange change = waitForChange(I_EXTX | I_ESTOP);

      if (change.turnedOn(I_EXTX)) {
        if (xPressedCount < 8) {
          xPressedCount++;
          blinkFast(1);
        } else {
          blinkFast(3);
        }
      }

      if (change.turnedOff(I_ESTOP)) {
        break;
      }
    }

    setExtLed(false);

    if (xPressedCount == 0) {
      blinkFast(3);
      delay(200);
      // Repeat over.
    } else {
      // Display to the user
      delay(250); // some time to put hand away to see the LED
      blinkMedium(xPressedCount);
      return xPressedCount - 1;
    }
  }
}

void fillTubes() {
  while (true) {
    int valveIndex = enterValveIndexUnderEStop();

    // Now X turns the motor+valve on/off, and ESTOP is "go to next valve"
    while (true) {
      InputsChange change = waitForChange(I_EXTX | I_ESTOP);

      if (change.isChanged(I_EXTX)) {
        if (change.isOn(I_EXTX)) {
          enableValve(valveIndex);
          enableMotor();
        } else {
          disableMotor();
          delay(50);
          disableValves();
        } 
      }

      if (change.turnedOn(I_ESTOP)) {
        disableMotor();
        disableValves();
        break;
      }
    }

    if ((!getExtA()) || (getExtB())) {
      // Program selector not at "fill tubes" position on EStop: exit to program selection
      blinkSlow(3);
      waitForReset(I_ESTOP);
      delay(1000);
      return;
    }
  }
}

void displayDigit(int digit) {
  if (digit == 0) {
    blinkSlow(1);
  } else {
    blinkFast(digit);
  }
}

void calibration() {
  // Input valve number
  int valveIndex = -1;
  bool valveConfirmed = false;
  while (!valveConfirmed) {
    valveIndex = enterValveIndexUnderEStop();

    // Long X to confirm valve selection, ESTOP to re-enter

    while (!valveConfirmed) {
      InputsChange change = waitForChange(I_EXTX | I_ESTOP);
      if (change.turnedOn(I_EXTX)) {
        unsigned long pressedAt = millis();
        while (getExtX()) {
          unsigned long msNow = millis();
          if (msNow - pressedAt >= 3000) {
            valveConfirmed = true;
            blinkFast(1);
            waitForReset(I_EXTX);
            break;
          }
        }
      }

      // ESTOP means select another valve
      if ((!valveConfirmed) && change.turnedOn(I_ESTOP)) {
        break;
      }
    }
  }

  bool pourOnEStopRelease = true;
  while (true) {
    InputsChange change = waitForChange(I_EXTX | I_ESTOP);

    // X only works when no EStop
    if (change.turnedOn(I_EXTX) && (!getEStop())) {
      int countsPerLiter = getCountsPerLiter();
      if (getExtA()) {
        // Display
        delay(250); // (time to move hand away from the LED)
        displayDigit((countsPerLiter / 100) % 10);
        delay(1000);
        displayDigit((countsPerLiter / 10) % 10);
        delay(1000);
        displayDigit(countsPerLiter % 10);
      } else {
        // Edit
        const int STEP = 10;
        if (getExtB()) {
          // Increment
          if (countsPerLiter + STEP <= MAX_COUNTS_PER_LITER) {
            setCountsPerLiter(countsPerLiter + STEP);
            blinkFast(1);
          } else {
            blinkFast(3);
          }
        } else {
          // Decrement
          if (countsPerLiter - STEP >= MIN_COUNTS_PER_LITER) {
            setCountsPerLiter(countsPerLiter - STEP);
            blinkFast(1);
          } else {
            blinkFast(3);
          }
        }
      }
    }

    if (change.turnedOff(I_ESTOP)) {
      if (pourOnEStopRelease) {
        if (!getExtX()) { // manual override
          pour(valveIndex, 0.5);
        }
        pourOnEStopRelease = !getEStop();
      } else {
        // Next time.
        pourOnEStopRelease = true;
      }
    }
  }
}
