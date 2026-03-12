/*
  Reading multiple RFID tags, simultaneously!
  By: Nathan Seidle @ SparkFun Electronics
  Date: October 3rd, 2016
  https://github.com/sparkfun/Simultaneous_RFID_Tag_Reader

  Constantly reads and outputs any tags heard

  If using the Simultaneous RFID Tag Reader (SRTR) shield, make sure the serial slide
  switch is in the 'SW-UART' position
*/

#include "src/Config.h"
#include "src/WifiHelper.h"
#include "src/SocketClient.h" 
#include "src/SocketListener.h"
#include "src/RfidReader.h"
#include "src/Tag.h"
#include "src/TagTracker.h"

SocketClient socketClient;
WifiHelper wifi(&socketClient);
TagTracker tracker;
RfidReader rfid(&socketClient); // RFID reader object to capture tag reads
SocketListener listener(&wifi, &rfid, &socketClient); // Listens for configuration commands

bool isRfidSetup = false;

void waitForRaceClient() {
  socketClient.waitForRaceClient();
}

bool setupRfidAndRaceClient() {
#ifndef USE_TEST_DATA
  // Initialize RFID reader
  if (!rfid.begin(rfidBaudLow)) // Setup rfid
  {
    Serial.println(F("RFID reader failed to respond. Please check wiring."));
    // Adding this log statement and removing the infinite loop (eg. code) to
    // allow possibility of recovery via remote communication on socket.
    Serial.println(F("RFID reader setup failure!"));
    return false;
  } else {
    Serial.println(F("RFID reader setup success."));
  }

  // Stop reading immediately — C# will send StartReader (via SetAntennaGain) when it connects.
  // This prevents the reader from scanning when no C# app is connected.
  rfid.stopReading();
#endif

  waitForRaceClient();
  socketClient.sendDebugMessage("Found race client");
  return true;
}

void setup()
{
  Serial.begin(115200);
  
  // The following line can be uncommented for debugging with the Serial Monitor
  // Leave commented for normal use, as well as selecting Tools | USB Mode | Normal Mode
  //while (!Serial); // Wait for the serial port to come online
  
  wifi.init();
  listener.init();

  isRfidSetup = setupRfidAndRaceClient();
}

void loop()
{
    while(!isRfidSetup) {
      delay(3000);
      isRfidSetup = setupRfidAndRaceClient();
    }

  // Handle incoming config / control commands (runs in both normal and test mode)
  listener.check();

  // Handle commands typed in the Serial Monitor (e.g. StartReader, StopReader, EnterBootloader)
  if (Serial.available())
  {
    String cmd = Serial.readStringUntil('\n');
    cmd.trim();
    if (cmd.length() > 0)
    {
      if (DO_SERIAL) Serial.println("Serial command: " + cmd);
      listener.handleCommand(cmd);
    }
  }

#ifdef USE_TEST_DATA
  delay(2000);
  Serial.println("Sending test data");
  socketClient.sendTestData();
#else

      // Poll reader for new data
      switch (rfid.poll())
      {
        case RfidEvent::KeepAlive:
            while (tracker.hasPending())
            {
                Tag* tag = tracker.getPendingTag();
                if (!tag)
                    break;

                if (socketClient.sendTag(*tag))
                    tracker.markSent(tag);
                else
                    break; // Send failed — leave tag pending, retry next KeepAlive
            }
            break;
        case RfidEvent::TagFound:
          {
            Tag tag;
            if (rfid.getLastTag(tag))
            {
                tracker.onTagRead(tag);
            }
            break;  
          }
        case RfidEvent::Error:
            Serial.println("Unknown RFID error");
            break;
        case RfidEvent::None:
        default:
            break;
    }
#endif

    wifi.validateWifiConnection();
}
