#include <Arduino.h>
#include <Grove_LED_Bar.h>

Grove_LED_Bar led_bar(13, 12, 0);
int level = 10;

void setup() {
    Serial.begin(28800);
    pinMode(15, OUTPUT);
    digitalWrite(15, HIGH);

    led_bar.begin();
}

void loop() {
  led_bar.setLevel(level);
  if (level > 0) {
      level = level - 1;
  }
  delay(1000);
}
