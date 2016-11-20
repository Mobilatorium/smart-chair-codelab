#include <Arduino.h>
#include <Ultrasonic.h>
#include <Grove_LED_Bar.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>
#include <FirebaseArduino.h>
#include <HealthTracker.h>

#define HARDCODED_TIME_TO_DISCHARGE 1 * 60 * 1000
#define HARDCODED_TIME_TO_RECHARGE 30 * 1000
#define WIFI_SSID "Guest-SSID"
#define WIFI_PASSWORD "<wifi-password>"
#define FIREBASE_HOST "codelab1-ed543.firebaseio.com"
#define FIREBASE_SECRET "FkGmitQaMh0jV5QNvUuXBiE1HjXUd2eNXQH7fUen"

Ultrasonic ultrasonic = Ultrasonic(14);
Grove_LED_Bar led_bar = Grove_LED_Bar(13, 12, 0);
HealthTracker health_tracker = HealthTracker();

void setup() {
    Serial.begin(28800);

    pinMode(15, OUTPUT);
    digitalWrite(15, HIGH);

    led_bar.begin();

    WiFi.begin(WIFI_SSID); // use WiFi.begin(WIFI_SSID, WIFI_PASSWORD) for secured networks;
    Serial.print("WiFi: connecting");
    while (WiFi.status() != WL_CONNECTED) {
        Serial.print(".");
        delay(500);
    }
    Serial.println();
    Serial.println("WiFi: connected.");

    Firebase.begin(FIREBASE_HOST, FIREBASE_SECRET);

    Serial.println("setup finished");
}

void loop() {
    unsigned time_to_discharge = Firebase.getInt("time_to_discharge");
    if (time_to_discharge <= 0) {
        time_to_discharge = HARDCODED_TIME_TO_DISCHARGE;
    }
    unsigned time_to_recharge = Firebase.getInt("time_to_recharge");
    if (time_to_recharge <= 0) {
        time_to_recharge = HARDCODED_TIME_TO_RECHARGE;
    }
    health_tracker.Configure(time_to_discharge, time_to_recharge);

    float health = health_tracker.Tick(ultrasonic.MeasureInCentimeters() < 10);
    float health_bar_level = abs(health) < 1e-10 ? 0.5f : health * 10.f;

    led_bar.setLevel(health_bar_level);
    Serial.print("health = ");
    Serial.println(health);
    Firebase.setFloat("health", health);

    delay(500);
}
