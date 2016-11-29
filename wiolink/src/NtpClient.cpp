#include "NtpClient.h"

NtpClient::NtpClient() : _ntpIpAdress(216, 239, 35, 0) {
  _localPort = 2390;
}

void NtpClient::init() {
    Serial.println("NtpClient - Starting UDP");
    _udp.begin(_localPort);
    Serial.print("NtpClient - Local port: ");
    Serial.println(_udp.localPort());
}

unsigned long NtpClient::getNtpTime() {
    Serial.println("NtpClient - sending NTP packet...");
    //set all bytes in the buffer to 0
    memset(_packetBuffer, 0, NTP_PACKET_SIZE);

    //Initialize values needed to form NTP request
    //(see URL above for details on the packets)
    _packetBuffer[0] = 0b11100011;   //LI, Version, Mode
    _packetBuffer[1] = 0;     //Stratum, or type of clock
    _packetBuffer[2] = 6;     //Polling Interval
    _packetBuffer[3] = 0xEC;  //Peer Clock Precision
    //8 bytes of zero for Root Delay & Root Dispersion
    _packetBuffer[12]  = 49;
    _packetBuffer[13]  = 0x4E;
    _packetBuffer[14]  = 49;
    _packetBuffer[15]  = 52;

    //all NTP fields have been given values, now
    //you can send a packet requesting a timestamp:
    _udp.beginPacket(_ntpIpAdress, 123); //NTP requests are to port 123
    _udp.write(_packetBuffer, NTP_PACKET_SIZE);
    _udp.endPacket();
    delay(1000);

    int cb;

    bool packegRecieved = false;
    while (!packegRecieved) {
      delay(500);
      cb = _udp.parsePacket();
      if (cb) {
        packegRecieved = true;
        break;
      }
      Serial.println("NtpClient - no packet yet");
    }

    Serial.print("NtpClient - packet received, length=");
    Serial.println(cb);
    //We've received a packet, read the data from it
    _udp.read(_packetBuffer, NTP_PACKET_SIZE); // read the packet into the buffer

    //the timestamp starts at byte 40 of the received packet and is four bytes,
    //or two words, long. First, esxtract the two words:

    unsigned long highWord = word(_packetBuffer[40], _packetBuffer[41]);
    unsigned long lowWord = word(_packetBuffer[42], _packetBuffer[43]);
    //combine the four bytes (two words) into a long integer
    //this is NTP time (seconds since Jan 1 1900):
    unsigned long secsSince1900 = highWord << 16 | lowWord;
    Serial.print("NtpClient - Seconds since Jan 1 1900 = " );
    Serial.println(secsSince1900);

    //now convert NTP time into everyday time:
    Serial.print("NtpClient - Unix time = ");

    //subtract seventy years:
    unsigned long epoch = secsSince1900 - seventyYears;
    //print Unix time:
    Serial.println(epoch);

    return epoch;
}
