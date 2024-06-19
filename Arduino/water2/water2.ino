#include "WiFiS3.h"
#include "WiFiSSLClient.h"

#include "cert.h"
#include "D:\Bn\Src\Water_Secrets\wifi.h"

const char *ssid = WIFI_SSID;
const char *pass = WIFI_PASS;

int status = WL_IDLE_STATUS;
const char *serverDomain = "ec2-3-78-70-168.eu-central-1.compute.amazonaws.com";

WiFiSSLClient netClient;

void setup() {
  Serial.begin(9600);
  while (!Serial);

  if (WiFi.status() == WL_NO_MODULE) {
    Serial.println("Communication with WiFi module failed!");
    while (true);
  }

  while (status != WL_CONNECTED) {
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(ssid);
    status = WiFi.begin(ssid, pass);

    delay(5000);
  }

  netClient.setCACert(serverCert);
}

void sendRequest() {
  if (netClient.connect(serverDomain, 443)) {
    Serial.println("Sending HTTP request...");

    netClient.println("GET / HTTP/1.1");
    netClient.println("Host: ec2-3-78-70-168.eu-central-1.compute.amazonaws.com");
    netClient.println("Connection: close");
    //netClient.println("My Custom Header: bla-bla-bla"); just checked that server rejects
    netClient.println();
  }
}

void readResponse() {
  while (netClient.connected()) {
    while (netClient.available()) {
      char c = netClient.read();
      Serial.print(c);
    }
  }

  netClient.stop();
}

void loop() {
  while (true) {
    sendRequest();
    readResponse();

    Serial.println();
    delay(10000);
  }
}
