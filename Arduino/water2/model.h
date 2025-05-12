#pragma once

const int NUMBER_OF_VALVES = 8;
const int VOLUMEML_MAX = 3000;

typedef enum {
  TS_NOTSTARTED = 0,
  TS_SUCCESS = 2,
  TS_LOWRATE = 10,
  TS_NOCOUNTER = 11,
  TS_ERROR = 99
} TaskStatus;

struct TaskItem {
  int valveIndex;
  int volumeMl;
  TaskStatus status;
};

struct Task {
  long id;
  TaskItem items[NUMBER_OF_VALVES];
  int itemsCount;
};