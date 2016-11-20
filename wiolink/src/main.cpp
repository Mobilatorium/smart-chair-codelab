#include <Arduino.h>
#include <Ultrasonic.h>
#include <Grove_LED_Bar.h>
#include <HealthTracker.h>

#define HARDCODED_TIME_TO_DISCHARGE 60 * 1000
#define HARDCODED_TIME_TO_RECHARGE 20 * 1000

Ultrasonic ultrasonic = Ultrasonic(14);
Grove_LED_Bar led_bar = Grove_LED_Bar(13, 12, 0);
HealthTracker health_tracker = HealthTracker();

void setup() {
    Serial.begin(28800);

    pinMode(15, OUTPUT);
    digitalWrite(15, HIGH);

    led_bar.begin();
    health_tracker.Configure(HARDCODED_TIME_TO_DISCHARGE, HARDCODED_TIME_TO_RECHARGE);
}

void loop() {
    float health = health_tracker.Tick(ultrasonic.MeasureInCentimeters() < 10);
    float health_bar_level = abs(health) < 1e-10 ? 0.5f : health * 10.f;

    led_bar.setLevel(health_bar_level);
    Serial.print("health = ");
    Serial.println(health);

    delay(500);
}
