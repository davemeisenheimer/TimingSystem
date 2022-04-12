/*
  Reading multiple RFID tags, simultaneously!
  By: Nathan Seidle @ SparkFun Electronics
  Date: October 3rd, 2016
  https://github.com/sparkfun/Simultaneous_RFID_Tag_Reader

  Constantly reads and outputs any tags heard

  If using the Simultaneous RFID Tag Reader (SRTR) shield, make sure the serial slide
  switch is in the 'SW-UART' position
*/

#include <SoftwareSerial.h> //Used for transmitting to the device
#include "src/WifiHelper.h"
#include "src/SocketListener.h"
//#include "src/NanoReader.h"

#include "arduino_secrets.h"
#include "TimingSystem.h"

WiFiClient client;
WifiHelper wifi;

char socketServer[] = "192.168.0.39";
int socketServerPort = 13000;

SoftwareSerial softSerial(2, 3); // RX, TX

RFID nano;                             // Create instance
SocketListener listener(&nano, &wifi); // List for configuration commands

int foundTagCount = 0;

enum LapState
{
  // Individual is crossing the line and we are still getting increasing RSSI on each read
  LINE_DATA_GATHERING,
  // We've determined the line has been crossed (i.e. detected highest RSSI) but are still getting reads
  LINE_DATA_SENT,
  // The lap data was sent and we are no longer getting reads.  Awaiting the next lap detection
  LINE_DATA_DETECT
};

struct Tag
{
  byte tagEPC[64];
  byte tagEPCBytes;
  int tagRssi;
  unsigned long tagTimeStamp;
  unsigned long nextStateTime; // Arduino time (millis) at which we will timeout the current LapState
  LapState lapState;
};

#define SUPPORTED_TAG_COUNT 32
#define TAG_MAX_SEND_COUNT 6
Tag myTags[SUPPORTED_TAG_COUNT]; // All known tags

#define UNSET_RSSI -32000;

int pendingTags = 0;
unsigned long keepAliveTime;

// Only call this function if the WiFiClient is connected already
// Returns true if the tag has data that needs to be sent
boolean checkPendingTag(Tag *tag)
{
  if (millis() > tag->nextStateTime)
  {
    // Serial.print("lapState: ");
    // Serial.println(tag.lapState);
    if (tag->lapState == LINE_DATA_GATHERING)
    {
      // queue this puppy for sending. lapState will transition to LINE_DATA_SENT with
      // a successful send of the lap data to whomever our client is (socket?).
//      NanoReader::highBeep();
      return true;
    }

    if (tag->lapState == LINE_DATA_SENT)
    {
      // We've passed the short timeout for no longer detecting the tag, so we'll make a longer lap timeout
      // now.  Any detections due to loitering at the finish line will be ignored for the next 30s and we
      // will not detect another lap until that time has passed.
      tag->nextStateTime = millis() + 30000;
      tag->lapState = LINE_DATA_DETECT; // Ready to detect the next lap
      pendingTags--;
    }
  }
  return false;
}

void sendTagData(Tag *tag)
{
  // Start communication with the number of bytes in the EPC
  client.println(tag->tagEPCBytes);
  for (byte y = 0; y < tag->tagEPCBytes; y++)
  {
    client.println(tag->tagEPC[y], HEX);
  }

  client.println(tag->tagTimeStamp);
  client.println(END_TIMING_MESSAGE);

  tag->lapState = LINE_DATA_SENT;
  tag->tagRssi = -32000;
}

// "
// 12
// 2260027105911462148125448
// ##$$##\r\n"
void checkPendingTags()
{
  Tag *tags2Send[4];
  int tags2SendCount = 0;
  boolean success = false;

  for (int x = 0; x < foundTagCount; x++)
  {
    if (checkPendingTag(&myTags[x]))
    {
      tags2Send[tags2SendCount] = &myTags[x];
      tags2SendCount++;
    }
  }

  if (tags2SendCount > 0)
  {
    // if (client.connected())
    if (client.connect(socketServer, socketServerPort))
    {
      for (int i = 0; i < tags2SendCount; i++)
      {
        sendTagData(tags2Send[i]);
      }
      success = true;
    }

    if (success)
    {
      client.stop();
    }
  }
}

#define DO_SERIAL true

// We got data from a tag we recognize, so here we will make some decisions about
// whether to log that data for future consideration or send it along to the socketServer
// to record a lap.
void logTagInfo(int tIdx, int rssi, unsigned long timeStamp)
{
  boolean isStronger = rssi >= myTags[tIdx].tagRssi;

  if (DO_SERIAL)
  {
    Serial.print("Millis: ");
    Serial.print(millis());
    Serial.print("; nextStateTime: ");
    Serial.println(myTags[tIdx].nextStateTime);
    Serial.print("; lapState: ");
    Serial.println(myTags[tIdx].lapState);

    Serial.print("logging for: ");
    Serial.print(tIdx);
    Serial.print("; lapState: ");
    Serial.print(myTags[tIdx].lapState);
    Serial.print("; rssi: ");
    Serial.print(rssi);
    Serial.print("; tagRssi: ");
    Serial.print(myTags[tIdx].tagRssi);
    Serial.print("; isStronger: ");
    Serial.print(isStronger);

    Serial.print("timeStamp: ");
    Serial.println(timeStamp);
  }

  bool isTime2Detect = millis() > myTags[tIdx].nextStateTime && myTags[tIdx].lapState == LINE_DATA_DETECT;

  if (isStronger &&
      (myTags[tIdx].lapState == LINE_DATA_GATHERING || isTime2Detect))
  {
    if (DO_SERIAL)
    {
      Serial.println("Setting tag data for sending");
    }

    if (isTime2Detect)
    {
      pendingTags++;
    }

    if (myTags[tIdx].lapState == LINE_DATA_DETECT)
//      NanoReader::lowBeep();

    myTags[tIdx].lapState = LINE_DATA_GATHERING;
    myTags[tIdx].tagRssi = rssi;
    myTags[tIdx].tagTimeStamp = timeStamp;
    // Ensure that we keep looking for stronger RSSI to ensure that the data we
    // send represents a crossing of the finish line rather than an approach to
    // the finish line. i.e. we won't leave the LINE_DATA_GATHERING state yet
    myTags[tIdx].nextStateTime = millis() + 350;

    Serial.print("timeStamp: ");
    Serial.println(myTags[tIdx].tagTimeStamp);
  }
}

int findMyTag(byte tagEpc[], size_t byteCount)
{
  for (int x = 0; x < foundTagCount; x++)
  {
    if (myTags[x].tagEPCBytes == byteCount)
    {
      if (memcmp(tagEpc, myTags[x].tagEPC, byteCount) == 0)
      {
        return x;
      }
    }
  }

  myTags[foundTagCount].tagEPCBytes = byteCount;
  memcpy(myTags[foundTagCount].tagEPC, tagEpc, byteCount);
  foundTagCount++;

  // Didn't find the tag
  return foundTagCount - 1;
}

void setup()
{
  Serial.begin(9600);
  while (!Serial)
    ; // Wait for the serial port to come online
  wifi.init();
  listener.init();

  for (int x = 0; x < SUPPORTED_TAG_COUNT; x++)
  {
    myTags[x].nextStateTime = 0;
    myTags[x].lapState = LINE_DATA_DETECT;
    myTags[x].tagRssi = UNSET_RSSI;
  }

  if (setupNano(38400) == false) // Configure nano to run at 38400bps
  {
    Serial.println(F("Module failed to respond. Please check wiring."));
    while (1)
      ; // Freeze!
  }

  nano.setRegion(REGION_NORTHAMERICA); // Set to North America

  nano.setReadPower(500); // 5.00 dBm. Higher values may caues USB port to brown out
  // Max Read TX Power is 27.00 dBm and may cause temperature-limit throttling

  // Serial.println(F("Press a key to begin scanning for tags."));
  // while (!Serial.available())
  //   ;            //Wait for user to send a character
  // Serial.read(); //Throw away the user's character

  waitForRaceClient();
  nano.startReading(); // Begin scanning for tags
  keepAliveTime = millis();
}

void loop()
{
  listener.check(); // Check for incoming configuration commands

  if (nano.check() == true) // Check to see if any new data has come in from module
  {
    byte responseType = nano.parseResponse(); // Break response into tag ID, RSSI, frequency, and timestamp

    if (responseType == RESPONSE_IS_KEEPALIVE || responseType == RESPONSE_IS_TEMPTHROTTLE || responseType == RESPONSE_IS_TEMPTHROTTLE || responseType == RESPONSE_IS_TEMPERATURE)
    {
      keepAliveTime = millis();
      checkPendingTags();
    }
    else if (responseType == RESPONSE_IS_TAGFOUND)
    {
      // If we have a full record we can pull out the fun bits
      int rssi = nano.getTagRSSI(); // Get the RSSI for this tag read

      //      long freq = nano.getTagFreq(); //Get the frequency this tag was detected at

      unsigned long timeStamp = nano.getTagTimestamp(); // Get the time this was read, (ms) since last keep-alive message

      byte tagEPCBytes = nano.getTagEPCBytes(); // Get the number of bytes of EPC from response

      byte tagEPC[64];

      if (DO_SERIAL)
      {
        // Print EPC bytes, this is a subsection of bytes from the response/msg array
        Serial.print(F(" epc["));
      }

      for (byte x = 0; x < tagEPCBytes; x++)
      {
        if (nano.msg[31 + x] < 0x10)
          if (DO_SERIAL)
          {
            Serial.print(F("0")); // Pretty print
            Serial.print(nano.msg[31 + x], HEX);
            Serial.print(F(" "));
          }

        tagEPC[x] = nano.msg[31 + x];
      }

      if (DO_SERIAL)
      {
        Serial.print(F("]"));
        Serial.println();
      }

      int myTagIndex = findMyTag(tagEPC, tagEPCBytes);

      if (myTagIndex > -1 && myTagIndex < SUPPORTED_TAG_COUNT)
      {
        // We've got that tag!
        logTagInfo(myTagIndex, rssi, (timeStamp + keepAliveTime));
      }
    }
    else if (responseType == ERROR_CORRUPT_RESPONSE)
    {
      Serial.println("Bad CRC");
    }
    else
    {
      // Unknown response
      Serial.print("Unknown error");
    }
  }

  wifi.validateWifiConnection();
}

// Gracefully handles a reader that is already configured and already reading continuously
// Because Stream does not have a .begin() we have to do this outside the library
boolean setupNano(long baudRate)
{
  nano.begin(softSerial); // Tell the library to communicate over software serial port

  // Test to see if we are already connected to a module
  // This would be the case if the Arduino has been reprogrammed and the module has stayed powered
  softSerial.begin(baudRate); // For this test, assume module is already at our desired baud rate
  while (softSerial.isListening() == false)
    ; // Wait for port to open

  // About 200ms from power on the module will send its firmware version at 115200. We need to ignore this.
  while (softSerial.available())
    softSerial.read();

  nano.getVersion();

  if (nano.msg[0] == ERROR_WRONG_OPCODE_RESPONSE)
  {
    // This happens if the baud rate is correct but the module is doing a ccontinuous read
    nano.stopReading();

    Serial.println(F("Module continuously reading. Asking it to stop..."));

    delay(1500);
  }
  else
  {
    // The module did not respond so assume it's just been powered on and communicating at 115200bps
    softSerial.begin(115200); // Start software serial at 115200

    nano.setBaud(baudRate); // Tell the module to go to the chosen baud rate. Ignore the response msg

    softSerial.begin(baudRate); // Start the software serial port, this time at user's chosen baud rate

//    NanoReader::init();
    delay(250);
  }

  // Test the connection
  nano.getVersion();
  if (nano.msg[0] != ALL_GOOD)
    return (false); // Something is not right

  // The M6E has these settings no matter what
  nano.setTagProtocol(); // Set protocol to GEN2

  nano.setAntennaPort(); // Set TX/RX antenna ports to 1

  return (true); // We are ready to rock
}

void waitForRaceClient()
{
  bool raceClientReady = false;

  while (true)
  {
    if (client.connect(socketServer, socketServerPort))
    {
      client.print("READER_READY_");
      client.println(END_TIMING_MESSAGE);
      client.println();
      client.stop();
      break;
    }
  }
  Serial.println("Found race client.  Ready to race!");
}
