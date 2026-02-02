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

bool setupRfidAndRaceClient() {
#ifndef USE_TEST_DATA
  // Initialize RFID reader
  if (!rfid.begin(rfidBaudLow)) // Setup and start reading
  {
    Serial.println(F("RFID reader failed to respond. Please check wiring."));
    // Adding this log statement and removing the infinite loop (eg. code) to
    // allow possibility of recovery via remote communication on socket.
    Serial.println(F("RFID reader setup failure!"));
    return false;
  } else {
    Serial.println(F("RFID reader setup success."));  
  }
#endif

  socketClient.waitForRaceClient();
  return true;
}

void loop()
{
    while(!isRfidSetup) {
      delay(3000);
      isRfidSetup = setupRfidAndRaceClient();
    }
    
#ifdef USE_TEST_DATA
  delay(5000);
  Serial.println("Sending test data");
  socketClient.sendTestData();
#else 
 
  // 1. Handle incoming config / control commands
  listener.check(); // Check for incoming configuration commands
  
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
