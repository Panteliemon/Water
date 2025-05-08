#include "model.h"
#include "networking.h"

void setup() {
  pinMode(13, OUTPUT);
  digitalWrite(13, HIGH);
  delay(250);
  digitalWrite(13, LOW);

  Serial.begin(9600);
  while (!Serial);

  initNetwork();
}

void loop() {
  Task currentTask;

  tryGetNextTask(currentTask);

  delay(20000);
}
