#ifndef SOCKETLISTENER_H_
#define SOCKETLISTENER_H_

#include <WiFi.h>
#include "WifiHelper.h"
#include "RfidReader.h"
#include "SocketClient.h"

#define SOCKET_LOCAL_PORT 13001

class RFID;  // forward declaration

class SocketListener
{
public:
    SocketListener(WifiHelper* wifiHelper, RfidReader* rfidReader, SocketClient *socketClient);

    void init();
    bool check();
    void handleCommand(const String& command);

private:
    // Core collaborators
    WifiHelper* wifiHelper;
    WiFiServer server;
    RfidReader* rfidReader;
    SocketClient* socketClient;

    // ---- Internal helpers ----
    void handleClient(WiFiClient& client);

    int getCommandValue(String command);
    String getCommandValueStr(String command);
};

#endif
