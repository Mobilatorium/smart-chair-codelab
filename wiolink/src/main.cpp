#include <Arduino.h>
#include <Ultrasonic.h>
#include <Grove_LED_Bar.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ESP8266WebServer.h>
#include <FirebaseArduino.h>
#include <HealthTracker.h>
#include <NtpClient.h>

#define HARDCODED_TIME_TO_DISCHARGE_MS 1 * 60 * 1000
#define HARDCODED_TIME_TO_RECHARGE_MS 30 * 1000
#define HARDCODED_THRESHOLD_DISTANCE_CM 30
#define WIFI_SSID "HANDSOME"
#define WIFI_PASSWORD "handsomeisawesome"
#define FIREBASE_HOST "codelab1-ed543.firebaseio.com"
#define FIREBASE_SECRET "FkGmitQaMh1jV5QNvUuXBiE1HjXUd2eNXQH7fUen"

Ultrasonic ultrasonic = Ultrasonic(14);
Grove_LED_Bar led_bar = Grove_LED_Bar(13, 12, 0);
HealthTracker health_tracker;
unsigned long start_unix_timestamp_in_seconds = 0;

void setup() {
    Serial.begin(28800);

    while (!Serial) {
        // wait serial port initialization
    }

    pinMode(15, OUTPUT);
    digitalWrite(15, HIGH);

    led_bar.begin();

    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
    Serial.print("WiFi: connecting");
    while (WiFi.status() != WL_CONNECTED) {
        Serial.print(".");
        delay(500);
    }
    Serial.println();
    Serial.println("WiFi: connected.");

    NtpClient ntp_client;
    ntp_client.init();
    start_unix_timestamp_in_seconds = ntp_client.getNtpTime();

    Firebase.begin(FIREBASE_HOST, FIREBASE_SECRET);
    Serial.println("setup finished");
}

void loop() {
    uint32_t now_local_timestamp = millis();

    uint32_t time_to_discharge = Firebase.getInt("time_to_discharge_ms");
    if (time_to_discharge <= 0) {
        time_to_discharge = HARDCODED_TIME_TO_DISCHARGE_MS;
    }
    uint32_t time_to_recharge = Firebase.getInt("time_to_recharge_ms");
    if (time_to_recharge <= 0) {
        time_to_recharge = HARDCODED_TIME_TO_RECHARGE_MS;
    }
    bool reconfigure = health_tracker.Configure(now_local_timestamp, time_to_discharge, time_to_recharge);

    uint32_t threshold_distance_cm = Firebase.getInt("threshold_distance_cm");
    if (threshold_distance_cm <= 0) {
      threshold_distance_cm = HARDCODED_THRESHOLD_DISTANCE_CM;
    }

    uint32_t distance = ultrasonic.MeasureInCentimeters();

    float health = health_tracker.Tick(distance < threshold_distance_cm, now_local_timestamp);
    float health_bar_level = abs(health) < 1e-10 ? 0.5f : health * 10.f;

    led_bar.setLevel(health_bar_level);

    StaticJsonBuffer<200> jsonBuffer;

    JsonObject& log_json = jsonBuffer.createObject();
    log_json.set("start_unix_timestamp_in_seconds", start_unix_timestamp_in_seconds);
    log_json.set("reconfigure", reconfigure);
    log_json.set("now_local_timestamp", now_local_timestamp);
    log_json.set("distance", distance);
    log_json.set("health", health);
    log_json.set("threshold_distance_cm", threshold_distance_cm);
    log_json.set("time_to_recharge_ms", time_to_recharge);
    log_json.set("time_to_discharge_ms", time_to_discharge);

    char log_json_char[log_json.measureLength() + 1];
    log_json.printTo(log_json_char, sizeof(log_json_char));
    log_json.printTo(Serial);
    Serial.println();

    Firebase.setFloat("health", health);
    Firebase.push("/log", log_json);

    delay(1000);
}
