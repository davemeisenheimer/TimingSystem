#ifndef SOCKETLISTENER_H_
#define SOCKETLISTENER_H_

#include "SparkFun_UHF_RFID_Reader.h" //Library for controlling the M6E Nano module
#include <WiFiNINA.h>
#include "WifiHelper.h"
#include "../arduino_secrets.h"

#define SOCKET_LOCAL_PORT 13001

class SocketListener
{
#define BUFFER_SIZE 255

public:
    SocketListener(RFID *nano, WifiHelper *wifiHelper);

    void init();
    void check();

private:
    RFID *nano;
    WifiHelper *wifiHelper;
    WiFiServer server;

    bool continueListening;
    void setAntennaGain(char *command);

    String getCommandValueStr(String command);

    const int currentLineSize = BUFFER_SIZE;
    const int commandRequestSize = BUFFER_SIZE;
    char currentLine[BUFFER_SIZE];    // make a String to hold incoming data from the client
    char commandRequest[BUFFER_SIZE]; // the actual api request
};

#endif