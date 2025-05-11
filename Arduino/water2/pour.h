#pragma once

#include "model.h"

TaskStatus pour(int valveIndex, double volumeLiters);

// Read sensor's scaling factor
int getCountsPerLiter();
// Write sensor's scaling factor to EEPROM
void setCountsPerLiter(int value);

const int MIN_COUNTS_PER_LITER = 200;
const int MAX_COUNTS_PER_LITER = 500;