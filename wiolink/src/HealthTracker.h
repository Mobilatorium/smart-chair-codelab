#ifndef HealthTracker_h
#define HealthTracker_h

#include <Arduino.h>

class HealthTracker {

public:
  HealthTracker();
  void Configure(unsigned time_to_discharge, unsigned time_to_recharge);
  float Tick(bool is_sat);

private:
  unsigned time_to_discharge, time_to_recharge, last_tick_timestamp;
  float level;
};
#endif
