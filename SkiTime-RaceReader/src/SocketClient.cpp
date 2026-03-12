#include "SocketClient.h"
#include "SocketClientProtocol.h"
#include "Config.h"

SocketClient::SocketClient()
{
}

bool SocketClient::ensureConnected()
{
    if (client.connected())
        return true;

    if (DO_SERIAL) Serial.println("SocketClient: reconnecting...");
    client.stop();
    delay(250); // Give C# time to process the disconnect and return to Accept()
    client.connect(SOCKET_SERVER_IP, SOCKET_SERVER_PORT);
    // Return false even if connect succeeded: don't send immediately after reconnect.
    // The caller will retry on the next opportunity (e.g. next KeepAlive).
    return false;
}

void SocketClient::reconnectIfNeeded()
{
    if (client.connected())
        return;

    if (DO_SERIAL) Serial.println("SocketClient: periodic reconnect...");
    client.stop();
    delay(250);
    if (client.connect(SOCKET_SERVER_IP, SOCKET_SERVER_PORT))
    {
        if (DO_SERIAL) Serial.println("SocketClient: periodic reconnect succeeded");
    }
}

bool SocketClient::sendTag(const Tag& tag)
{
    if (!ensureConnected())
        return false;

    client.println(tag.epcBytes);

    for (int i = 0; i < tag.epcBytes; i++) {
        byte b = tag.epc[i];
        client.print((b >> 4) & 0x0F, HEX);  // high nibble
        client.print(b & 0x0F, HEX);         // low nibble
    }

    client.println();
    client.println(tag.timestamp);
    client.println(END_TIMING_MESSAGE);
    client.println();
    client.flush();

    if (DO_SERIAL) Serial.println("sendTag: sent EPC bytes: " + String(tag.epcBytes));
    return true;
}

bool SocketClient::sendReady()
{
    if (!ensureConnected())
        return false;

    if (DO_SERIAL) Serial.println("sendReady: connected");
    client.println(END_READY_MESSAGE);
    client.println();
    client.flush();
    return true;
}

void SocketClient::sendDebugMessage(const String& message)
{
  #ifdef DO_SEND_DEBUG_MSG
  if (!client.connected())
      return;

  client.println(message);
  client.println(END_DEBUG_MESSAGE);
  client.println();
  client.flush();
  #endif
}


void SocketClient::sendHeartbeat()
{
    if (!client.connected())
        return;

    client.println(END_HEARTBEAT_MESSAGE);
    client.println();
    client.flush();
}

void SocketClient::waitForRaceClient()
{
  sendDebugMessage("Waiting for race client");
  while (true)
  {
    if (this->sendReady())
      break;
    delay(500);
  }
}

void SocketClient::sendTestData()
{
    const char hexStr[] = "2019112911861A01101001D8";
    byte tagEPC[64];
    memset(tagEPC, 0, sizeof(tagEPC));
    int tagEPCBytes = hexStringToByteArray(hexStr, tagEPC, sizeof(tagEPC));
    unsigned long timeStamp = millis();

    if (!ensureConnected())
        return;

    client.println(tagEPCBytes);

    for (byte y = 0; y < tagEPCBytes; y++) {
        byte b = tagEPC[y];
        client.print((b >> 4) & 0x0F, HEX);
        client.print(b & 0x0F, HEX);
    }

    client.println();
    client.println(timeStamp);
    client.println(END_TIMING_MESSAGE);
    client.println();
    client.flush();

    if (DO_SERIAL) Serial.println("sendTestData: sent test tag");
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
