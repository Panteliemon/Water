#pragma once

const int NUMBER_OF_VALVES = 8;
const int VOLUMEML_MAX = 3000;

typedef enum {
  NOTSTARTED = 0,
  SUCCESS = 2,
  LOWRATE = 10,
  NOCOUNTER = 11
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