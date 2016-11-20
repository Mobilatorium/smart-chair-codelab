#include "HealthTracker.h"

HealthTracker::HealthTracker() {
    this->level = 1.0f;
    this->time_to_discharge = INFINITY;
    this->time_to_recharge = INFINITY;
    this->last_tick_timestamp = millis();
}

void HealthTracker::Configure(unsigned time_to_discharge, unsigned time_to_recharge) {
    if (this->time_to_discharge != time_to_discharge || this->time_to_recharge != time_to_recharge) {
        this->level = 1.0f;
        Serial.printf("Params will be updated to time_to_discharge = %d, time_to_recharge = %d\n",
            time_to_discharge, time_to_recharge);
    }
    this->time_to_discharge = time_to_discharge;
    this->time_to_recharge = time_to_recharge;
}

float HealthTracker::Tick(bool is_sat) {
    unsigned now_timestamp = millis();
    unsigned tick_delta = now_timestamp - last_tick_timestamp;
    last_tick_timestamp = now_timestamp;
    if (is_sat) {
        level = max(0.0f, level - (float) tick_delta / (float) time_to_discharge);
    } else {
        level = min(1.0f, level + (float) tick_delta / (float) time_to_recharge);
    }
    return level;
}
