#ifndef WIFIHELPLER_H_
#define WIFIHELPLER_H_

#include <WiFiNINA.h>
#include "../arduino_secrets.h"

class WifiHelper
{ ///////please enter your sensitive data in the Secret tab/arduino_secrets.h
public:
    WifiHelper();

    void init();

    void recordMessage();
    void recordLastChar();
    void recordElapsedWifi();

    unsigned long getWifiEpoch();
    void validateWifiConnection();
    void stopValidation();

    int wifiRestarts;
    int serverRestarts;
    int invalidTimeSyncs;

    WiFiServer server;

private:
    const unsigned long WATCHDOG_NO_MSG_SECS = 600; // 1 hour is 3600 secs. 10 min is 600 sec.
    const unsigned long WATCHDOG_KICK_SERVER_SECS = WATCHDOG_NO_MSG_SECS + 1;
    const unsigned long WATCHDOG_FULL_REBOOT_TIME = 86400; // 1 day

    /// Goodbye cruel world, I'll see you in the next life.
    /// I'll be coming back as a chicken.
    static void reboot()
    {
        //  CPU_CCP = 0xD8;
        //  WDT.CTRLA = 0x4;
        //  while (true)
        //  {
        //  }
    }

    void printWifiStatus();
    int restartWifi();

    int lastWdogCheckSecs;
    int currentSecs;
    unsigned long lastCharWifiMs;
    unsigned long lastWifiMsgSecs;

    int status;
    int serverStatus;
    bool keepValidating;
};

#endif