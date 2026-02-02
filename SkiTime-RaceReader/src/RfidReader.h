#ifndef RFIDREADER_H_
#define RFIDREADER_H_

#include <Arduino.h>
#include "SparkFun_UHF_RFID_Reader.h"

#include "SocketClient.h"
#include "Tag.h"

// Pin configuration for ESP32 Nano reader
#define RFID_RX_PIN 16
#define RFID_TX_PIN 17

#define MODULE_TYPE ThingMagic_M7E_HECTO
#define BUZZER1 10
#define BUZZER2 9

// Constants used by the ThingMagic reader
// #define REGION_NORTHAMERICA 0xA0
// #define ERROR_WRONG_OPCODE_RESPONSE 0x01
// #define ALL_GOOD 0x00
// #define ERROR_CORRUPT_RESPONSE 0x02

// Response types
// #define RESPONSE_IS_KEEPALIVE 0x10
// #define RESPONSE_IS_TAGFOUND  0x11
// #define RESPONSE_IS_TEMPTHROTTLE 0x12
// #define RESPONSE_IS_TEMPERATURE 0x13

enum class RfidEvent
{
    None,
    KeepAlive,
    TagFound,
    Error
};

class RfidReader
{
public:
    RfidReader(SocketClient *socketClient);

    bool begin(long rfidBaud);                     // Setup and initialize the reader
    RfidEvent poll();                 // Check for new messages from the readerx

    bool isKeepAlive() const;
    bool isTagFound() const;

    bool getLastTag(Tag &tag);

    void getAbsoluteTimestamp(Tag &tag);

    void sendTestData();

    // Hardware-specific feedback
    void lowBeep();
    void highBeep();

    void setAntennaGain(const int gain);
    void stopReading();
    void startReading();

    // Delete this!!!!!!!!
    void getReadPower();

private:
    SocketClient* socketClient;
    RFID reader;
    HardwareSerial& rfidSerial;
    byte lastResponse;
    #define rfidBaudLow 38400
    #define rfidBaudHigh 115200

    int getRSSI();
    unsigned long getTimestamp();
    int getEpc(byte* buffer, int maxLen);
    

    unsigned long keepAliveBaseMillis;
};

#endif
