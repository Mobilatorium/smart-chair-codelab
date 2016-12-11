#ifndef TimeNTP_h
#define TimeNTP_h

#include <Arduino.h>
#include <Time.h>
#include <Ethernet.h>
#include <WiFiUdp.h>
#include <ESP8266WiFi.h>

// enum TimeFormate {
//     DD_MM_YYYY_HH_MM_SS = 0,
//     YYYY_MM_DD_HH_MM_SS = 1,
// };

enum ErrorStatus {
    ER_OK = 0,
    ER_FAILED_CONNECT_TO_NTP_SERVER = 1,
    ER_FAILED_CONNECT_TO_WIFI = 2,
};

// NTP Servers:
static const char ntpServerName[] = "us.pool.ntp.org";
//static const char ntpServerName[] = "time.nist.gov";
//static const char ntpServerName[] = "time-a.timefreq.bldrdoc.gov";
//static const char ntpServerName[] = "time-b.timefreq.bldrdoc.gov";
//static const char ntpServerName[] = "time-c.timefreq.bldrdoc.gov";

const int timeZone = 1;     // Central European Time
//const int timeZone = -5;  // Eastern Standard Time (USA)
//const int timeZone = -4;  // Eastern Daylight Time (USA)
//const int timeZone = -8;  // Pacific Standard Time (USA)
//const int timeZone = -7;  // Pacific Daylight Time (USA)

const int NTP_PACKET_SIZE = 48; // NTP time is in the first 48 bytes of message
byte packetBuffer[NTP_PACKET_SIZE]; //buffer to hold incoming & outgoing packets

class TimeNTP {

public:
    TimeNTP();

    uint64_t GetTimeUnix();
    String GetTimeStamp();
    ErrorStatus Error();

    //this method is public for casting it to function pointer
    //  for setSyncProvider from Time library
    time_t GetNtpTime();

private:
    ErrorStatus error;

    WiFiUDP Udp;
    void SendNTPpacket(IPAddress &address);

};

extern TimeNTP Time;

#endif
