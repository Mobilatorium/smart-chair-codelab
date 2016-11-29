#ifndef HealthTracker_h
#define HealthTracker_h

#include <Arduino.h>

class HealthTracker {

public:
  HealthTracker();
  bool Configure(uint32_t start_timestamp, uint32_t time_to_discharge, uint32_t time_to_recharge);
  float Tick(bool is_sat, uint32_t now_timestamp);

private:
  uint32_t time_to_discharge, time_to_recharge, last_tick_timestamp;
  float level;
};
#endif
