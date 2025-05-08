#include "WiFiS3.h"
#include "WiFiSSLClient.h"

#include "networking.h"
#include "cert.h"
#include "D:\Bn\Src\Water_Secrets\wifi.h"
#include "D:\Bn\Src\Water_Secrets\apikey-orduino.h"
#include "Buffer.h"

const char *ssid = WIFI_SSID;
const char *pass = WIFI_PASS;
const char *apiKey = API_KEY;

const char *serverDomain = "ec2-63-176-210-31.eu-central-1.compute.amazonaws.com";

int wlStatus = WL_IDLE_STATUS;
WiFiSSLClient netClient;
Buffer requestBuffer;
Buffer responseBuffer;

void initNetwork() {
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
}

bool sendPostRequest(const char *route, Buffer &message, Buffer *response) {
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
    while (netClient.connected()) {
      while (netClient.available()) {
        char c = netClient.read();

        // TODO first line is like
        // HTTP/1.1 403 Forbidden
        // Then find
        // Content-Type: text/plain; charset=utf-8
        // Content-Length: 2
        
        // Line breaks are 13-10.

        Serial.print(c); // for gruesome debug only
      }
    }

    netClient.stop();
    return true;
  } else {
    return false;
  }
}

bool tryGetNextTask(Task &t) {
  requestBuffer.setLength(0);

  sendPostRequest("/operation/nexttask", requestBuffer, 0);
}