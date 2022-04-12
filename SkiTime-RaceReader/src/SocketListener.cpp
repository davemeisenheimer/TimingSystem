#include "SocketListener.h"

SocketListener::SocketListener(RFID *nano, WifiHelper *wifiHelper) : server(SOCKET_LOCAL_PORT),
                                                                     nano(nano),
                                                                     wifiHelper(wifiHelper),
                                                                     continueListening(true)
{
}

void SocketListener::init()
{
  this->server.begin();
}

void SocketListener::check()
{
  if (!this->continueListening)
    return;

  WiFiClient client = this->server.available();
  if (client)
  {
    // String currentLine = "";
    currentLine[0] = '\0';
    commandRequest[0] = '\0';
    while (client.connected())
    {
      if (client.available())
      {
        char c = client.read();
        if (c == '\n')
        { // if the byte is a newline character

          // if the current line is blank, you got two newline characters in a row.
          // that's the end of the client HTTP request, so send a response:
          if (strlen(currentLine) == 0)
          {
            client.println("We're good");

            Serial.print("commandRequest is: ");
            Serial.println(commandRequest);

            if (strstr(commandRequest, "SetAntennaGain") != NULL)
            {
              this->setAntennaGain(commandRequest);
              Serial.println("Antenna gain was set");
              client.println("Antenna gain was set");
            }
            else if (strstr(commandRequest, "StopWifiValidation") != NULL)
            {
              this->wifiHelper->stopValidation();
              client.println("Arduino will no longer validate wifi connection");
            }
            else if (strstr(commandRequest, "StopListenting") != NULL)
            {
              this->continueListening = false;
              client.println("Arduino has stopped listening.");
            }
            else
            {
              client.print("Unknown arduino command: ");
              client.println(currentLine);
            }

            // break out of the while loop:
            break;
          }
          else
          { // if you got a newline, then clear currentLine:
            currentLine[0] = '\0';
          }
        }
        else if (c != '\r')
        { // if you got anything else but a carriage return character,
          int currLen = strlen(currentLine);
          if (currLen < this->currentLineSize - 2)
          {
            currentLine[currLen] = c;
            currentLine[currLen + 1] = '\0';

            commandRequest[0] = '\0';
            strcpy(commandRequest, currentLine);
          }
        }
      }
    }

    client.println();
    client.flush();
    client.stop();

    Serial.println("client disconnected");
  }
}

void SocketListener::setAntennaGain(char *command)
{
  String gainStr = this->getCommandValueStr(command);
  int gain = gainStr.toInt();

  if (gain < 27 && gain > 4)
  {
    gain = gain * 100;
    Serial.print("Setting antenna gain to: ");
    Serial.println(gain);
    nano->stopReading();
    nano->setReadPower((gain));
    nano->startReading();
  }
}

String SocketListener::getCommandValueStr(String command)
{
  String value = command.substring(command.indexOf(",") + 1);
  return value;
}