#pragma once
#include <WiFi.h>
#include "Tag.h"

class SocketClient
{
public:
    SocketClient();

    bool sendTag(const Tag& tag);
    bool sendReady();
    void sendTestData();
    void waitForRaceClient();
    void sendDebugMessage(const String& message);

private:
    WiFiClient client;

    byte hexCharToNibble(char c);
    int hexStringToByteArray(const char* hex, byte* output, size_t maxLen);
};