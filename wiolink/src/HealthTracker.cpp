#include "HealthTracker.h"

HealthTracker::HealthTracker() {

}

bool HealthTracker::Configure(uint32_t start_time_stamp, uint32_t time_to_discharge, uint32_t time_to_recharge) {
    if (this->time_to_discharge != time_to_discharge || this->time_to_recharge != time_to_recharge) {
        Serial.printf("Params will be updated to time_to_discharge = %d, time_to_recharge = %d\n",
            time_to_discharge, time_to_recharge);
        this->level = 1.0f;
        this->last_tick_timestamp = start_time_stamp;
        this->time_to_discharge = time_to_discharge;
        this->time_to_recharge = time_to_recharge;
        return true;
    }
    this->time_to_discharge = time_to_discharge;
    this->time_to_recharge = time_to_recharge;
    return false;
}

float HealthTracker::Tick(bool is_sat, uint32_t now_timestamp) {
    uint32_t tick_delta = now_timestamp - last_tick_timestamp;
    last_tick_timestamp = now_timestamp;
    if (is_sat) {
        level = max(0.0f, level - (float) tick_delta / (float) time_to_discharge);
    } else {
        level = min(1.0f, level + (float) tick_delta / (float) time_to_recharge);
    }
    return level;
}
