#include "SocketListener.h"
#include "SocketClient.h"
#include "Config.h"

SocketListener::SocketListener(
    WifiHelper *wifiHelper, 
    RfidReader *rfidReader,
    SocketClient *socketClient) 
    : server(SOCKET_LOCAL_PORT),
            wifiHelper(wifiHelper),
            rfidReader(rfidReader),
            socketClient(socketClient),
            continueListening(true)
{
    rfidReader = rfidReader;
}

void SocketListener::init()
{
  this->server.begin();
}

void SocketListener::check()
{
    if (!continueListening)
        return;

    WiFiClient client = server.available();
    if (!client)
        return;

    handleClient(client);

    client.stop();
    if (DO_SERIAL) socketClient->sendDebugMessage("SocketListener: client disconnected");
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

    if (DO_SERIAL) socketClient->sendDebugMessage("SocketListener command: " + command);

    handleCommand(command);
}

void SocketListener::handleCommand(const String& command)
{
    if (command.startsWith("SetAntennaGain"))
    {
        int gain = getCommandValue(command);
        rfidReader->setAntennaGain(gain);
    }
    else if (command == "StartReader")
    {
        rfidReader->startReading();
    }
    else if (command == "StopReader")
    {
        rfidReader->stopReading();
    }
    else if (command == "StopWifiValidation")
    {
        wifiHelper->stopValidation();
    }
    else if (command == "StopListening")
    {
        continueListening = false;
    }
    else if (command == "Reset")
    {
        this->server.stop();
        rfidReader->stopReading();
        ESP.restart();
    }
    else
    {
        if (DO_SERIAL) socketClient->sendDebugMessage("Unknown command: " + command);
    }
}

// Returns the integer value after the comma in the command string
// NB: A return value of -1 indicates no value found. Kind of flaky but good enough for now.
int SocketListener::getCommandValue(String command)
{
  String valueStr = getCommandValueStr(command);

  if (valueStr.length() == 0) {
    if (DO_SERIAL) socketClient->sendDebugMessage("No value found in command: " + command);
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
    if (DO_SERIAL) socketClient->sendDebugMessage("No value found in command: " + command);
    return "";
  }

  return command.substring(comma + 1);
}