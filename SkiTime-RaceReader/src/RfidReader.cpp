#include "RfidReader.h"
#include "Config.h"

RfidReader::RfidReader(SocketClient *socketClient)
    : lastResponse(0), 
        socketClient(socketClient),
        rfidSerial(Serial0),
        keepAliveBaseMillis(0)
{
    // Initialize buzzer pins
    pinMode(BUZZER1, OUTPUT);
    pinMode(BUZZER2, OUTPUT);
    digitalWrite(BUZZER2, LOW); // Half-buzzer grounding
}

void RfidReader::lowBeep()
{
    tone(BUZZER1, 130, 150);
}

void RfidReader::highBeep()
{
    tone(BUZZER1, 2093, 150);
}

bool RfidReader::begin(long rfidBaud)
{
    // You can specify the RX and TX pins for Serial communication, if you want.
    // rfidSerial.begin(rfidBaud, SERIAL_8N1, RFID_RX_PIN, RFID_TX_PIN);
    rfidSerial.begin(rfidBaud, SERIAL_8N1);
    reader.begin(rfidSerial, MODULE_TYPE);
    // reader.enableDebugging(Serial); // Pipe raw communication to the Serial Monitor

    delay(100);  
    
    //About 200ms from power on the module will send its firmware version at 115200. We need to ignore this.
    while (rfidSerial.available())
        rfidSerial.read();

    reader.getVersion();
    if (reader.msg[0] == ERROR_WRONG_OPCODE_RESPONSE)
    {
        //This happens if the baud rate is correct but the module is doing a ccontinuous read
        reader.stopReading();
        if (DO_SERIAL) socketClient->sendDebugMessage(F("Module continuously reading. Asking it to stop..."));
        delay(1500);
    }
    else
    {
        //The module did not respond so assume it's just been powered on and communicating at 115200bps
        rfidSerial.begin(rfidBaudHigh); //Start serial at 115200
        reader.setBaud(rfidBaud); //Tell the module to go to the chosen baud rate. Ignore the response msg
        rfidSerial.begin(rfidBaud); //Start the serial port, this time at user's chosen baud rate
        delay(250);
    }

    reader.getVersion();
    int responseCode = reader.msg[0];
    if (responseCode != ALL_GOOD) {
        if (DO_SERIAL) {
            socketClient->sendDebugMessage("RfidReader::begin: Reader failed to respond correctly. returned: " 
                            + String(ERROR_WRONG_OPCODE_RESPONSE, HEX));
            socketClient->sendDebugMessage("RfidReader::begin: Reader failed to respond correctly. returned: " 
                            + String(responseCode, HEX));

            socketClient->sendDebugMessage("RfidReader::begin: msg[2]: " 
                            + String(reader.msg[2], HEX));
            if (responseCode == ERROR_WRONG_OPCODE_RESPONSE)
            {
                    socketClient->sendDebugMessage("RfidReader::begin: ERROR_WRONG_OPCODE_RESPONSE returned was: " 
                                    + String(reader.msg[2], HEX));
            }
        }

        return false;
    }

    reader.setRegion(REGION_NORTHAMERICA);
    reader.setTagProtocol();
    reader.setAntennaPort();
    reader.startReading();

    return true;
}

RfidEvent RfidReader::poll()
{
    if (!reader.check())
        return RfidEvent::None;

    lastResponse = reader.parseResponse();

    if (lastResponse == ERROR_CORRUPT_RESPONSE)
    {
        if (DO_SERIAL) socketClient->sendDebugMessage("RfidReader::poll: Bad CRC returned from reader.parseResponse()");
        return RfidEvent::Error;
    }

    if (isKeepAlive()) {
        keepAliveBaseMillis = millis();
        return RfidEvent::KeepAlive;
    }

    if (isTagFound())
        return RfidEvent::TagFound;

    return RfidEvent::None;
}

bool RfidReader::isKeepAlive() const
{
    return lastResponse == RESPONSE_IS_KEEPALIVE ||
           lastResponse == RESPONSE_IS_TEMPTHROTTLE ||
           lastResponse == RESPONSE_IS_TEMPERATURE;
}

bool RfidReader::isTagFound() const
{
    return lastResponse == RESPONSE_IS_TAGFOUND;
}

int RfidReader::getRSSI()
{
    return reader.getTagRSSI();
}

unsigned long RfidReader::getTimestamp()
{
    return reader.getTagTimestamp();
}

int RfidReader::getEpc(byte* buffer, int maxLen)
{
    byte count = reader.getTagEPCBytes();
    if (count > maxLen)
        count = maxLen;

    for (byte i = 0; i < count; i++)
        buffer[i] = reader.msg[31 + i];

    return count;
}

bool RfidReader::getLastTag(Tag& tag)
{
    if (!isTagFound())
        return false;

    byte epcLen = reader.getTagEPCBytes();
    tag.epcBytes = epcLen;

    for (byte i = 0; i < epcLen; i++)
        tag.epc[i] = reader.msg[31 + i];

    tag.rssi = reader.getTagRSSI();

    getAbsoluteTimestamp(tag);

    if (DO_SERIAL)
    {
        String msg = " epc[";
        for (byte i = 0; i < epcLen; i++)
        {
            if (tag.epc[i] < 0x10) msg = msg + "0";
            msg = msg + String(tag.epc[i], HEX);
            msg = msg + " ";
        }
        msg = msg + "]";
        if (DO_SERIAL) socketClient->sendDebugMessage(msg);
    }

    return true;
}

// Keep-alive signals from the reader are used as a "base time" reference.
// The ThingMagic reader resets its internal tag timestamps to 0 after a reset
// or power cycle, so we store the last keepAlive millis() in the ino and
// add it to any tag timestamp to get a continuous absolute timestamp.
void RfidReader::getAbsoluteTimestamp(Tag &tag)
{
    tag.timestamp =
        keepAliveBaseMillis + reader.getTagTimestamp();
}

// Delete this!!!!!!!!!!!!!!!!!!
void RfidReader::getReadPower() {
  reader.getReadPower();
  int powerCdBm = (reader.msg[6] << 8) | reader.msg[7]; 
  if (DO_SERIAL) socketClient->sendDebugMessage("RfidReader: Antenna currently set to: " + String(powerCdBm) + " dBm");

  String msg = msg + " Bytes: ";
  for (int i = 0; i < 8; i++) {
    msg = msg + String(reader.msg[i], HEX) + " ";
  }
  if (DO_SERIAL) socketClient->sendDebugMessage(msg);
}

void RfidReader::setAntennaGain(const int gain)
{
    reader.stopReading();
    delay(100);

    if (DO_SERIAL) {
        getReadPower();
        socketClient->sendDebugMessage("RfidReader: Setting antenna gain to: " + String(gain) + " cdBm");
    }

    if (gain < 2700 && gain > 500)
    {
        reader.setReadPower((gain));
        reader.setWritePower((gain));

        if(DO_SERIAL) {
            socketClient->sendDebugMessage("NOW: ");
            getReadPower();
        }
    }

    reader.startReading();
}

void RfidReader::startReading()
{
    reader.startReading();
}

void RfidReader::stopReading()
{
    reader.stopReading();
}
