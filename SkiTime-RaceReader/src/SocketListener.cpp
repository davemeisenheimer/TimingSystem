#include "SocketListener.h"
#include "SocketListenerProtocol.h"
#include "SocketClient.h"
#include "Config.h"
#include "esp32s3/rom/rtc.h"

SocketListener::SocketListener(
    WifiHelper *wifiHelper, 
    RfidReader *rfidReader,
    SocketClient *socketClient) 
    : server(SOCKET_LOCAL_PORT),
            wifiHelper(wifiHelper),
            rfidReader(rfidReader),
            socketClient(socketClient)
{
}

void SocketListener::init()
{
  this->server.begin();
}

bool SocketListener::check()
{
    WiFiClient client = server.available();
    if (!client)
        return false;

    handleClient(client);

    client.stop();
    socketClient->sendDebugMessage("SocketListener: client disconnected");
    return true;
}

void SocketListener::handleClient(WiFiClient& client)
{
    String commandBuilder = "";
    String command = "";

    while (client.connected())
    {
        if (!client.available())
            continue;

        char c = client.read();

        if (c == '\n')
        {
            commandBuilder.trim();   // removes \r if present

            if (commandBuilder.length() == 0)
            {
                // Blank line = end of request
                client.println("We're good");
                break;
            }

            command = commandBuilder;
            commandBuilder = "";
        }
        else
        {
            commandBuilder += c;
        }
    }

    socketClient->sendDebugMessage("SocketListener command: " + command);

    handleCommand(command);
}

void SocketListener::handleCommand(const String& command)
{
    if (command.startsWith(CMD_SET_ANTENNA_GAIN))
    {
        int gain = getCommandValue(command);
        rfidReader->setAntennaGain(gain);
    }
    else if (command == CMD_START_READER)
    {
        rfidReader->startReading();
    }
    else if (command == CMD_STOP_READER)
    {
        rfidReader->stopReading();
    }
    else if (command == CMD_STOP_WIFI)
    {
        wifiHelper->stopValidation();
    }
    else if (command == CMD_RESET)
    {
        this->server.stop();
        rfidReader->stopReading();
        ESP.restart();
    }
    else if (command == CMD_ENTER_BOOTLOADER)
    {
        this->server.stop();
        rfidReader->stopReading();
        REG_WRITE(RTC_CNTL_OPTION1_REG, RTC_CNTL_FORCE_DOWNLOAD_BOOT);
        esp_restart();
    }
    else
    {
        socketClient->sendDebugMessage("Unknown command: " + command);
    }
}

// Returns the integer value after the comma in the command string
// NB: A return value of -1 indicates no value found. Kind of flaky but good enough for now.
int SocketListener::getCommandValue(String command)
{
  String valueStr = getCommandValueStr(command);

  if (valueStr.length() == 0) {
    socketClient->sendDebugMessage("No value found in command: " + command);
    return -1;
  }

  int value = valueStr.toInt();

  return value;
}

String SocketListener::getCommandValueStr(String command)
{  
  int comma = command.indexOf(',');
  if (comma < 0 || comma == command.length() - 1)
  {
    socketClient->sendDebugMessage("No value found in command: " + command);
    return "";
  }

  return command.substring(comma + 1);
}