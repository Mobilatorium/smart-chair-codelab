#include <Arduino.h>

void setup() {
    // инициализация Serial порта c baudrate равным 28800
    Serial.begin(28800);

    // Для включения GPIO портов на плате WioLink необходимо подать высокий сигнал на 15-ый пин микропроцессора.
    pinMode(15, OUTPUT);
    digitalWrite(15, HIGH);
}

void loop() {
    Serial.println("Hello World! Devfest Siberia 2016 rulezzzz");
    delay(1000);
}
