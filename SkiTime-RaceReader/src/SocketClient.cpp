#include "SocketClient.h"
#include "SocketClientProtocol.h"
#include "Config.h"

SocketClient::SocketClient()
{
}

bool SocketClient::sendTag(const Tag& tag)
{
    if (DO_SERIAL) this->sendDebugMessage("sendTag: Connecting to client");

    // Don't use sendDebugMessage after client.connect, or we get a recursive loop!
    if (!client.connect(SOCKET_SERVER_IP, SOCKET_SERVER_PORT))
        return false;

    client.println(tag.epcBytes);
    
    for (int i = 0; i < tag.epcBytes; i++) {
      byte b = tag.epc[i];
      client.print((b >> 4) & 0x0F, HEX);  // high nibble
      client.print(b & 0x0F, HEX);         // low nibble
    }

    // for (int i = 0; i < tag.epcBytes; i++)
    //     client.println(tag.epc[i], HEX);


    client.println();

    client.println(tag.timestamp);

    client.println(END_TIMING_MESSAGE);

    client.println();

    client.flush();
    client.stop();

    // Now we can use sendDebugMessage again
    if (DO_SERIAL) this->sendDebugMessage("Connected to client");
    if (DO_SERIAL) this->sendDebugMessage("EPC bytes: " + String(tag.epcBytes));
    return true;
}

bool SocketClient::sendReady()
{
  if (DO_SERIAL) this->sendDebugMessage("waitForRaceClient");
  
  bool raceClientReady = false;

  if (client.connect(SOCKET_SERVER_IP, SOCKET_SERVER_PORT))
  {
      // Next debug msg can just use Serial, since client is getting this message anyway
      if (DO_SERIAL) Serial.println("waitForRaceClient: client connected");
      client.println(END_READY_MESSAGE);
      client.println(); 
      client.flush();
      client.stop();
      raceClientReady = true;
  }

  if (DO_SERIAL) this->sendDebugMessage("Found race client: " + String(raceClientReady ? "yes" : "no"));
  return raceClientReady;
}

void SocketClient::sendDebugMessage(const String& message)
{
  if (DO_SERIAL) Serial.println("sendDebugMessage sending: " + message);
  
  bool messageSent = false;

  if (client.connect(SOCKET_SERVER_IP, SOCKET_SERVER_PORT))
  {
      if (DO_SERIAL) Serial.println("sendDebugMessage: client connected");
      client.println(message);
      client.println(END_DEBUG_MESSAGE);
      client.println(); 
      client.flush();
      client.stop();
      messageSent = true;
  }

  if (DO_SERIAL) Serial.println("Debug message sent: " + String(messageSent ? "yes" : "no"));
}


void SocketClient::waitForRaceClient()
{
  while (true)
  {
    if (this->sendReady())
      break;
    delay(500);
  }
}

void SocketClient::sendTestData()
{   
  int rssi = -44; // Get the RSSI for this tag read
  unsigned long timeStamp = millis();

  byte tagEPC[64];
  const char hexStr[] = "2019112911861A01101001D8";
  memset(tagEPC, 0, sizeof(tagEPC));

  int tagEPCBytes = hexStringToByteArray(hexStr, tagEPC, sizeof(tagEPC));

  bool success = false;
  if (client.connect(SOCKET_SERVER_IP, SOCKET_SERVER_PORT))
  {
    if (DO_SERIAL) Serial.println("Connected to client");

    client.println(tagEPCBytes);
    if (DO_SERIAL) Serial.println(String(tagEPCBytes));

    for (byte y = 0; y < tagEPCBytes; y++) {
      byte b = tagEPC[y];
      client.print((b >> 4) & 0x0F, HEX);  // high nibble
      client.print(b & 0x0F, HEX);         // low nibble

      if (DO_SERIAL) {
        Serial.println(String((b >> 4) & 0x0F, HEX) + String(b & 0x0F, HEX));
      }
    }

    client.println();
    if (DO_SERIAL) Serial.println("");

    client.println(timeStamp);
    if (DO_SERIAL) Serial.println(String(timeStamp));

    client.println(END_TIMING_MESSAGE);
    if (DO_SERIAL) Serial.println(END_TIMING_MESSAGE);

    client.println();
    if (DO_SERIAL) Serial.println("");
    client.flush();
    client.stop();
  }
}

byte SocketClient::hexCharToNibble(char c)
{
  if (c >= '0' && c <= '9') return c - '0';
  if (c >= 'A' && c <= 'F') return c - 'A' + 10;
  if (c >= 'a' && c <= 'f') return c - 'a' + 10;
  return 0;  // invalid character
}

int SocketClient::hexStringToByteArray(const char* hex, byte* output, size_t maxLen)
{
  size_t len = strlen(hex);
  size_t byteCount = len / 2;
  if (byteCount > maxLen) byteCount = maxLen;

  for (size_t i = 0; i < byteCount; i++)
     {
      output[i] =
          (hexCharToNibble(hex[2 * i]) << 4) |
           hexCharToNibble(hex[2 * i + 1]);
     }
  return byteCount;
}
