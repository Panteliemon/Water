#include <Arduino.h>
#include "hardware.h"
#include "blink.h"

unsigned long lastBlinkEndMs = 0;

void debounceBlinks(int minimumInterval) {
  unsigned long msNow = millis();
  unsigned long minimumIntervalEnd = lastBlinkEndMs + (unsigned long)minimumInterval;
  if (msNow < lastBlinkEndMs + minimumIntervalEnd) {
    delay(minimumIntervalEnd - msNow);
  }
}

void blinkFast(int numberOfTimes) {
  debounceBlinks(170);
  for (int i=0; i<numberOfTimes; i++) {
    if (i > 0) {
      delay(170);
    }
    setExtLed(true);
    delay(80);
    setExtLed(false);
    lastBlinkEndMs = millis();
  }
}

void blinkMedium(int numberOfTimes) {
  debounceBlinks(250);
  for (int i=0; i<numberOfTimes; i++) {
    if (i > 0) {
      delay(250);
    }
    setExtLed(true);
    delay(250);
    setExtLed(false);
    lastBlinkEndMs = millis();
  }
}

void blinkSlow(int numberOfTimes) {
  debounceBlinks(250);
  for (int i=0; i<numberOfTimes; i++) {
    if (i > 0) {
      delay(1000);
    }
    setExtLed(true);
    delay(1000);
    setExtLed(false);
    lastBlinkEndMs = millis();
  }
}