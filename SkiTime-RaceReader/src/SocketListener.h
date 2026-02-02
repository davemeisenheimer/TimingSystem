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
    void check();

private:
    // Core collaborators
    WifiHelper* wifiHelper;
    WiFiServer server;
    RfidReader* rfidReader;
    SocketClient* socketClient;

    bool continueListening;

    // ---- Internal helpers ----
    void handleClient(WiFiClient& client);
    void handleCommand(const String& command);

    int getCommandValue(String command);
    String getCommandValueStr(String command);
};

#endif
