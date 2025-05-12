#include "WiFiS3.h"
#include "WiFiSSLClient.h"

#include "networking.h"
#include "cert.h"
#include "D:\Bn\Src\Water_Secrets\wifi.h"
#include "D:\Bn\Src\Water_Secrets\apikey-orduino.h"
#include "Buffer.h"
#include "ResponseReader.h"
#include "pour.h"

const char *ssid = WIFI_SSID;
const char *pass = WIFI_PASS;
const char *apiKey = API_KEY;

const char *serverDomain = "ec2-63-176-210-31.eu-central-1.compute.amazonaws.com";

typedef enum {
  HS_OK = 0,
  HS_NETWORKERROR = 1,
  HS_BADRESPONSE = 2,
  HS_STATUSCODE = 3
} HttpStatus;

int wlStatus = WL_IDLE_STATUS;
WiFiSSLClient netClient;
bool wasInitNetwork = false;
Buffer requestBuffer;
Buffer responseBuffer;

void initNetwork() {
  if (!wasInitNetwork) {
    if (WiFi.status() == WL_NO_MODULE) {
      Serial.println("Communication with WiFi module failed!");
      while (true);
    }

    while (wlStatus != WL_CONNECTED) {
      Serial.print("Attempting to connect to SSID: ");
      Serial.println(ssid);
      wlStatus = WiFi.begin(ssid, pass);

      delay(5000);
    }

    netClient.setCACert(serverCert);

    wasInitNetwork = true;
  }
}

HttpStatus sendPostRequest(const char *route, Buffer &message, Buffer *response) {
  if (netClient.connect(serverDomain, 443)) {
    Serial.println("Sending HTTP request...");

    netClient.print("POST ");
    netClient.print(route);
    netClient.println(" HTTP/1.1");
    netClient.println("Accept: */*");
    netClient.println("Connection: close");
    netClient.print("Host: ");
    netClient.println(serverDomain);
    netClient.println("Cache-Control: no-cache");
    netClient.print("Water2-ApiKey: ");
    netClient.println(apiKey);
    
    message.ensureTerminalZero();
    netClient.print("Content-Length: ");
    netClient.println(message.getLength() - 1); // 1 for terminal zero

    if (message.getLength() > 1) { // non empty when strip away terminal zero
      netClient.println("Content-Type: text/plain");
      netClient.println();
      netClient.println(message.p());
    }

    netClient.println();

    // Grab response
    Serial.println("Reading response...");
    ResponseReader responseReader(&responseBuffer);
    while (netClient.connected()) {
      while (netClient.available()) {
        char c = netClient.read();
        //Serial.print(c); // for gruesome debug only

        responseReader.nextByte(c);
      }
    }

    netClient.stop();

    if (responseReader.isParsedSuccessfully()) {
      Serial.print("Response parsed OK. Code ");
      Serial.println(responseReader.getStatusCode());

      if (responseReader.getStatusCode() == 200) {
        return HS_OK;
      } else {
        return HS_STATUSCODE;
      }
    } else {
      Serial.println("[x] Could not parse response");
      return HS_BADRESPONSE;
    }
  } else {
    return HS_NETWORKERROR;
  }
}

void sendError() {
  requestBuffer.setLength(0);
  sendPostRequest("/operation/error", requestBuffer, 0);
}

// If RW pointer of the buffer is NOT at last position, decreases it by 1.
void shiftBackIfNotLast(Buffer &buffer) {
  if (buffer.getPos() < buffer.getLength()) {
    buffer.seek(buffer.getPos() - 1);
  }
}

bool tryParseTask(Buffer &buffer, Task &t) {
  buffer.seek(0);

  char c = 0;
  bool wasTaskId = false;
  t.itemsCount = 0;

  while (true) {
    if (!buffer.tryReadChar(c)) {
      return wasTaskId;
    }

    if (c == 'T') {
      if (wasTaskId) {
        return false;
      }

      wasTaskId = true;
      long taskId = 0;
      if (!buffer.tryReadDecimalLong(taskId)) {
        return false;
      }

      // Task id == 0 is allowed.

      // Move 1 backwards in order to read the symbol which terminated the taskId again
      // on the next iteration into the variable c.
      shiftBackIfNotLast(buffer);

      t.id = taskId;
    } else if (c == 'I') {
      if (t.itemsCount == NUMBER_OF_VALVES) {
        // Too many items in one task, stop parse but consider it successful parse
        return wasTaskId;
      }

      int parsedValveIndex = 0;
      if (!buffer.tryReadDecimalInt(parsedValveIndex)) {
        return false;
      }
      shiftBackIfNotLast(buffer);

      if ((parsedValveIndex < 0) || (parsedValveIndex >= NUMBER_OF_VALVES)) {
        return false;
      }

      t.items[t.itemsCount].valveIndex = parsedValveIndex;
      t.items[t.itemsCount].volumeMl = 0;
      t.items[t.itemsCount].status = TS_NOTSTARTED;
      t.itemsCount++;
      
      // We allow missing "V" parameter, interpret as 0.
    } else if (c == 'V') {
      // Not allowed before first "I" parameter
      if (t.itemsCount == 0) {
        return false;
      }

      // We allow duplicate "V" parameter, overwrite.

      int parsedVolumeMl = 0;
      if (!buffer.tryReadDecimalInt(parsedVolumeMl)) {
        return false;
      }
      shiftBackIfNotLast(buffer);

      if ((parsedVolumeMl < 0) || (parsedVolumeMl > VOLUMEML_MAX)) {
        // Rather to shorting to 3000 and pouring all 50 L on the floor within the first day, generate error.
        return false;
      }

      t.items[t.itemsCount - 1].volumeMl = parsedVolumeMl;
    } else {
      // Skip through unknown until the next known control char
    }
  }
}

// Limits spamming the server with error messages
int tryGetNextTask_badResponseCounter = 0;

bool tryGetNextTask(Task &t) {
  requestBuffer.setLength(0);
  responseBuffer.setLength(0);

  // Some info about ourselves
  requestBuffer.writeChar('C');
  requestBuffer.writeDecimalInt(getCountsPerLiter());

  HttpStatus httpStatus = sendPostRequest("/operation/nexttask", requestBuffer, &responseBuffer);
  if (httpStatus == HS_OK) {
    tryGetNextTask_badResponseCounter = 0;

    if (responseBuffer.getLength() >= 2) {
      if ((responseBuffer.p()[0] == ':') && (responseBuffer.p()[1] == ')')) {
        // Official server's response "there are no tasks for you r/n"
        return false;
      }
    }

    if (!tryParseTask(responseBuffer, t)) {
      sendError();
      return false;
    }
    
    return true;
  } else if (httpStatus == HS_BADRESPONSE) {
    // Limit spamming the server with error messages:
    if (tryGetNextTask_badResponseCounter < 2) {
      tryGetNextTask_badResponseCounter++;
      sendError();
    }
    return false;
  } else {
    // No connection or error status code: do nothing.
    tryGetNextTask_badResponseCounter = 0;
    return false;
  }
}

void reportTaskResult(Task &t, int itemIndex) {
  if ((itemIndex >= 0) && (itemIndex < NUMBER_OF_VALVES) && (itemIndex < t.itemsCount)) {
    // Don't report zeroth state
    if (t.items[itemIndex].status != TS_NOTSTARTED) {
      // Ok
      requestBuffer.setLength(0);
      requestBuffer.writeChar('T');
      requestBuffer.writeDecimalLong(t.id);
      requestBuffer.writeChar('I');
      requestBuffer.writeDecimalInt(t.items[itemIndex].valveIndex);
      requestBuffer.writeChar('R');
      requestBuffer.writeDecimalInt(t.items[itemIndex].status);

      sendPostRequest("/operation/taskresult", requestBuffer, 0);
      // Response doesn't matter, just send and that's it.
    }
  }
}