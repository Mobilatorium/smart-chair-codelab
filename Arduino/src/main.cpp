//
// Copyright 2015 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

// FirebaseDemo_ESP8266 is a sample that demo the different functions
// of the FirebaseArduino API.
#include <Arduino.h>
#include <Ultrasonic.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <FirebaseArduino.h>

// Set these to run example.
#define FIREBASE_HOST "firebasehost"
#define FIREBASE_AUTH "firebaseauth"
#define WIFI_SSID "wifisid"
#define WIFI_PASSWORD "wifipassword"

Ultrasonic ultrasonic(5);
int maxDistanceSM = 20;
int iterationPerSec=10;
bool isFree = false;
long startDuration = 0;
long iterationBatchDuration = 1000;

void setup() {
  Serial.begin(28800);
  pinMode(15, OUTPUT);
  digitalWrite(15, HIGH);
  // connect to wifi.
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  Serial.print("connecting");
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(500);
  }
  Serial.println();
  Serial.print("connected: ");
  Serial.println(WiFi.localIP());

  Firebase.begin(FIREBASE_HOST, FIREBASE_AUTH);
  maxDistanceSM = Firebase.getInt("maxDistanceSM");
  Serial.print("MaxDistanceSM = ");
  Serial.println(maxDistanceSM);
  iterationPerSec = Firebase.getInt("iterationPerSec");
  Serial.print("iterationPerSec = ");
  Serial.println(iterationPerSec);

  iterationBatchDuration = Firebase.getInt("iterationBatchDuration");
  Serial.print("iterationBatchDuration = ");
  Serial.println(iterationBatchDuration);
}

void loop() {
 int count = 0;
 for(int i=0; i<iterationPerSec;i++){
   long  rangeInCentimeters = ultrasonic.MeasureInCentimeters(); // two measurements should keep an interval
   Serial.println(rangeInCentimeters);
   if (rangeInCentimeters<=maxDistanceSM){
     count++;
   }
   delay((int)(iterationBatchDuration/iterationPerSec));
     Serial.print("millis=");
    Serial.println(millis());
 }
  // set value
  bool newFree = count<=(iterationPerSec/2);
  if (isFree != newFree){
    startDuration = millis();
  }

  int duration = millis() - startDuration;
  if (newFree){
    Firebase.setInt("freeTime", duration);
    Firebase.setInt("busyTime", 0);
    Serial.print("Freetime = ");
    Serial.println(duration);
  } else {
    Firebase.setInt("busyTime", duration);
    Firebase.setInt("freeTime", 0);
    Serial.print("BusyTime = ");
    Serial.println(duration);
  }
  isFree = newFree;
  // handle error
  if (Firebase.failed()) {
      Serial.print("setting /number failed:");
      Serial.println(Firebase.error());
      return;
  }

}
