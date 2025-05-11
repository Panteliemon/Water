#include "model.h"
#include "networking.h"
#include "hardware.h"
#include "pour.h"

void setup() {
  initHardware();

  Serial.begin(9600);
  while (!Serial);
}

void loop() {
  // Program selection
  if (getExtA()) {
    if (getExtB()) {
      while (true) ;
    } else {
      fillTubes();
    }
  } else {
    if (getExtB()) {
      while (true) ;
    } else {
      initNetwork();
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
      delay(80);
    }
    setExtLed(true);
    delay(50);
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

void fillTubes() {
  bool demandEStopDown = true;
  while (true) {
    if (demandEStopDown) {
      setExtLed(true);
      waitForSet(I_ESTOP);
      setExtLed(false);
    }

    // Valve selection while ESTOP pressed  
    int xPressedCount = 0;
    while (true) {
      InputsChange change = waitForChange(I_EXTX | I_ESTOP);

      if ((change.changedInputs & I_EXTX) != 0) {
        if ((change.inputsState & I_EXTX) != 0) {
          if (xPressedCount < 8) {
            xPressedCount++;
            setExtLed(true);
          } else {
            blinkFast(3);
          }
        } else {
          setExtLed(false);
        } 
      }

      if (((change.changedInputs & I_ESTOP) != 0) && ((change.inputsState & I_ESTOP) == 0)) {
        break;
      }
    }

    setExtLed(false);

    if (xPressedCount > 0) {
      // Indicate
      blinkMedium(xPressedCount);

      while (true) {
        InputsChange change = waitForChange(I_EXTX | I_ESTOP);

        if ((change.changedInputs & I_EXTX) != 0) {
          if ((change.inputsState & I_EXTX) != 0) {
            enableValve(xPressedCount - 1);
            enableMotor();
          } else {
            disableMotor();
            delay(50);
            disableValves();
          } 
        }

        if (((change.changedInputs & I_ESTOP) != 0) && ((change.inputsState & I_ESTOP) != 0)) {
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

      // Back to valve selection
      demandEStopDown = false;
    } else {
      // If valve's number not entered - send to the beginning
      demandEStopDown = true;
    }
  }
}
