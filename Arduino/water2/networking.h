#pragma once

#include "model.h"

// Never exits if fails to init
void initNetwork();

// If manages to obtain task from server - fills fields of t appropriately.
// If there are no tasks or if failed to parse server's response - false.
bool tryGetNextTask(Task &t);

// Reports result of execution for one item of the task only.
void reportTaskResult(Task &t, int itemIndex);
