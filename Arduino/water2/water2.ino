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

      // Carbonate puddle
      delay(10000);
      currentTask.items[i].status = SUCCESS;
      reportTaskResult(currentTask, i);
    }
  }

  delay(20000);
}
