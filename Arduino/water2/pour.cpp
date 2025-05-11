#include <Arduino.h>
#include <EEPROM.h>
#include "hardware.h"
#include "pour.h"

// Expected flow rate liters per min
const double EXPECTED_FLOW_RATE_LPM = 2.5;
// If measured flow rate drops under this level, "low rate" error is triggered.
const double LOW_RATE_THRESHOLD_ABS = 0.5;
const double LOW_RATE_THRESHOLD_LPM = EXPECTED_FLOW_RATE_LPM * LOW_RATE_THRESHOLD_ABS;
// Max allowed time in ms between start of the motor and counter registering the flow.
// If counter doesn't register anything - "no counter" error is triggered.
const unsigned long MAX_DELAY_PUMP_TO_COUNTER_MS = 3000;
const unsigned int COUNTS_TO_CONFIRM_COUNTER_CONNECTED = 2;
// Interval in ms in which we make a new flow rate calculation (does NOT equal to the interval _over_ which the flow is measured)
const unsigned int FLOW_RATE_CALCULATION_INTERVAL_MS = 150;
// Number of simultaneously performed staggered flow measurements.
// Flow will be measured over (this constant) times FLOW_RATE_CALCULATION_INTERVAL_MS ms.
const int SIMULTANEOUS_MEASUREMENTS_COUNT = 10;

int getCountsPerLiter() {
  int result = 0;
  EEPROM.get(0, result);
  if ((result < MIN_COUNTS_PER_LITER) || (result > MAX_COUNTS_PER_LITER)) {
    // Historical default values, newest to oldest: 420;//381;//343;//450;
    return 420;
  } else {
    return result;
  }
}

void setCountsPerLiter(int value) {
  if (value < MIN_COUNTS_PER_LITER) {
    value = MIN_COUNTS_PER_LITER;
  } else if (value > MAX_COUNTS_PER_LITER) {
    value = MAX_COUNTS_PER_LITER;
  }
  EEPROM.put(0, value);
}

TaskStatus pour(int valveIndex, double volumeLiters) {
  if ((volumeLiters <= 0) || (valveIndex < 0) || (valveIndex > 7)) {
    return TS_ERROR;
  }

  double countsPerLiter = (double)getCountsPerLiter();
  unsigned int totalRequiredPulses = (unsigned int)round(volumeLiters * countsPerLiter);
  if (totalRequiredPulses == 0) {
    return TS_ERROR;
  }
  
  wCounter = 0;
  enableValve(valveIndex);
  enableMotor();

  TaskStatus result = TS_SUCCESS;
  unsigned long startTime = millis();

  // Startup: we wait until counter starts counting, confirming that at least the device is connected
  while (wCounter < COUNTS_TO_CONFIRM_COUNTER_CONNECTED) {
    delay(5);

    unsigned long elapsedTime = millis() - startTime;
    if (elapsedTime >= MAX_DELAY_PUMP_TO_COUNTER_MS) {
      result = TS_NOCOUNTER;
      break;
    }
  }

  if (result == TS_SUCCESS) {
    // Started up ok. Start staggered measurements of flow rates

    // Historical data
    // Remembered values of counter
    unsigned int prevCounters[SIMULTANEOUS_MEASUREMENTS_COUNT];
    // Time in ms when the corresponding value was measured
    unsigned long timeStamps[SIMULTANEOUS_MEASUREMENTS_COUNT];

    for (int i=0; i<SIMULTANEOUS_MEASUREMENTS_COUNT; i++) {
      prevCounters[i] = 0xFFFF; // semantics for no data
      timeStamps[i] = 0;
    }

    // Put data N.0 into arrays right now
    unsigned long prevMeasurementTime = millis();
    prevCounters[0] = wCounter;
    timeStamps[0] = prevMeasurementTime;
    int dataIndex = 0; // index of current slot of historical data

    while (wCounter < totalRequiredPulses) {
      delay(5);

      // Fix volatiles
      unsigned long timeNow = millis();
      unsigned int counterNow = wCounter;

      // Whether or not it's time to make a new measurement
      unsigned long elapsedTime = timeNow - prevMeasurementTime;
      if (elapsedTime >= FLOW_RATE_CALCULATION_INTERVAL_MS) {
        // Go next data
        dataIndex = (dataIndex + 1) % SIMULTANEOUS_MEASUREMENTS_COUNT;

        unsigned int prevCounter = prevCounters[dataIndex];
        if (prevCounter != 0xFFFF) { // there is data in current slot
          double liters = ((double)(counterNow - prevCounter))/countsPerLiter;
          double minutes = ((double)(timeNow - timeStamps[dataIndex]))/60000.0;
          if (liters / minutes < LOW_RATE_THRESHOLD_LPM) {
            result = TS_LOWRATE;
            break;
          }
        }

        // Store current data
        prevCounters[dataIndex] = counterNow;
        timeStamps[dataIndex] = timeNow;
        
        // Measured.
        prevMeasurementTime = timeNow;
      }
    }
  }

  disableMotor();
  delay(50);
  disableValves();
  delay(100);

  return result;
}