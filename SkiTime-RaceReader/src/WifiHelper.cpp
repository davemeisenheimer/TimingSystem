#include "WifiHelper.h"

const IPAddress GATEWAY_IP(192, 168, 0, 1);
const IPAddress MY_IP(192, 168, 0, MY_IP_OCTET);
const IPAddress DNS_IP(206, 248, 154, 22);
const IPAddress SUBNET_IP(255, 255, 255, 0);

WifiHelper::WifiHelper() : lastWdogCheckSecs(0),
                           currentSecs(0),
                           wifiRestarts(0),
                           serverRestarts(0),
                           lastCharWifiMs(0),
                           lastWifiMsgSecs(0),
                           status(WL_IDLE_STATUS),
                           serverStatus(0),
                           server(80),
                           keepValidating(true)
{
}

void WifiHelper::init()
{
    char ssid[] = SECRET_SSID; // your network SSID (name)
    char pass[] = SECRET_PASS; // your network password (use for WPA, or use as key for WEP)

    // check for the WiFi module:
    if (WiFi.status() == WL_NO_MODULE)
    {
        Serial.println("Comms with WiFi module failed!");
        // don't continue
        while (true)
            ;
    }

    WiFi.config(MY_IP, DNS_IP, GATEWAY_IP, SUBNET_IP);

    // attempt to connect to Wifi network:
    // WiFi.config(MY_IP, DNS_IP, GATEWAY_IP, SUBNET_IP);
    while (this->status != WL_CONNECTED)
    {
        Serial.print("Try connect to: ");
        Serial.println(ssid); // print the network name (SSID);

        // Connect to WPA/WPA2 network. Change this line if using open or WEP network:
        this->status = WiFi.begin(ssid, pass);
        // wait 10 seconds for connection:
        delay(10000);
    }
    this->printWifiStatus(); // you're connected now, so print out the status

    this->server.begin();    // start the web server on port 80
    this->printWifiStatus(); // you're connected now, so print out the status
}

unsigned long WifiHelper::getWifiEpoch()
{
    unsigned long epoch;
    int numberOfTries = 0, maxTries = 6;

    do
    {
        epoch = WiFi.getTime();
        numberOfTries++;
    } while ((epoch == 0) && (numberOfTries < maxTries));

    if (numberOfTries == maxTries)
    {
        this->invalidTimeSyncs++;
        Serial.print("NTP unreachable!!");
    }
    else
    {
        Serial.print("Epoch received: ");
        Serial.println(epoch);
        Serial.println();
    }

    return epoch;
}

void WifiHelper::recordMessage()
{
    this->lastWifiMsgSecs = millis();
}

void WifiHelper::recordLastChar()
{
    this->lastCharWifiMs = millis();
}

void WifiHelper::recordElapsedWifi()
{
    // Decide if the wifi chip is hung somehow!
    unsigned long elapsedWifiMs = millis() - this->lastCharWifiMs;

    // check if delay has timed out after 10sec == 10000mS
    if (elapsedWifiMs >= 30000)
    {
        Serial.println("Wait too long for next client character");
    }
}

void WifiHelper::printWifiStatus()
{
    // print the SSID of the network you're attached to:
    Serial.print("SSID: ");
    Serial.println(WiFi.SSID());

    // print your board's IP address:
    IPAddress ip = WiFi.localIP();
    Serial.print("IP Address: ");
    Serial.println(ip);

    // print the received signal strength:
    long rssi = WiFi.RSSI();
    Serial.print("RSSI:");
    Serial.print(rssi);
    Serial.println(" dBm");
    // print where to go in a browser:
    Serial.print("IP: ");
    Serial.println(ip);
}

void WifiHelper::validateWifiConnection()
{
    if (!keepValidating)
        return;

    char ROUTER_IP[] = "192.168.0.1"; // Our local router for pinging.
    this->currentSecs = millis() / 1000;
    if ((this->currentSecs / 30) != (this->lastWdogCheckSecs / 30))
    {                      // Every ten seconds.
        Serial.print("."); // debug serial monitor heartbeat
        this->lastWdogCheckSecs = this->currentSecs;
        this->serverStatus = server.status();

        // Watchdog - check for no wifi action for too long.
        if ((this->currentSecs - this->lastWifiMsgSecs) > WATCHDOG_NO_MSG_SECS)
        {
            Serial.print("?"); // Indicate watchdog is looking
            this->serverStatus = server.status();

            // If the wifi is down, Disconnect and Restart the wifi
            if (WiFi.status() != WL_CONNECTED)
            {
                Serial.println("Wifi noconn. ");
                this->status = restartWifi();
            }
            else if (this->serverStatus != 1)
            { // Server status is no good
                // The Wifi is up, but the server is not ok
                // It might  be not non-idle if we are receiving or txing data, so be a bit patient
                // by setting client kick watchdog a bit longer than no msg timeout.
                // but we are here because of no polling messages, so chances are we have a problem.
                Serial.print("Server down.");
                Serial.print(this->serverStatus);
                if ((millis() / 1000 - this->lastWifiMsgSecs) > WATCHDOG_KICK_SERVER_SECS)
                { // WatchdogKickServer Timeout
                    Serial.println("Try server.begin");
                    this->serverRestarts += 1;
                    server.begin();
                    delay(1000); // wait 1 sec
                    if (server.status() != 1)
                    {
                        // server begin didn't fix server.status
                        Serial.println("Server begin didn't work Try wifiRestart");
                        this->status = restartWifi();
                    }
                    if (server.status() != 1)
                    {
                        Serial.print(server.status());
                        Serial.println("wifiRestart didn't fix svr. reboot");
                        WifiHelper::reboot();
                    }
                    this->lastWifiMsgSecs = (millis() / 1000); // start watchdog again.
                }
            }
            else
            { // Server status is good, but no messages and we don't know why. Try pinging the router.
                // If the ping fails restart wifi.
                // If we can't restart for a very long time, reset the board.
                Serial.print("Pinging ");
                Serial.print(ROUTER_IP);
                Serial.print(": ");
                this->status = WiFi.ping(ROUTER_IP);
                if (this->status >= 0)
                {
                    Serial.print("SUCCESS! RTT = ");
                    Serial.print(this->status);
                    Serial.println(" ms");
                    this->lastWifiMsgSecs = (millis() / 1000); // Network is ok. start watchdog again.
                }
                else
                { // ping failed
                    Serial.print("FAILED! Error code: ");
                    Serial.println(this->status);
                    //  The network seems to be down.  Try re-initializing it.
                    Serial.println(" Restarting Wifi");
                    this->status = restartWifi();

                    if ((millis() / 1000 - this->lastWifiMsgSecs) > WATCHDOG_FULL_REBOOT_TIME)
                    {
                        // This timer is long to avoid  rebooting just because the power is out in the house
                        // and takes out the router, for instance.
                        Serial.println(" No ping timeout. Reboot");
                        WifiHelper::reboot();
                    } // end full watchdog reboot
                }     // end ping failed
            }         // server status good but no messages, try ping
        }             // no wifi message timeout
    }                 // end every N secs examine watchdog
}

int WifiHelper::restartWifi()
{
    char ssid[] = SECRET_SSID; // your network SSID (name)
    char pass[] = SECRET_PASS; // your network password (use for WPA, or use as key for WEP)

    Serial.println("WIFI Con. Restarting");
    WiFi.disconnect();
    WiFi.end();
    Serial.println("WiFi restart");
    this->status = WL_IDLE_STATUS;
    this->wifiRestarts += 1;
    for (int n = 10; n > 0; n--)
    {
        // WiFi.config(MY_IP, DNS_IP, GATEWAY_IP, SUBNET_IP);
        Serial.print("Trying to connect to: ");
        Serial.println(ssid); // print the network name (SSID);

        // Connect to WPA/WPA2 network. Change this line if using open or WEP network:
        this->status = WiFi.begin(ssid, pass);
        // wait 10 seconds for connection:
        delay(10000);
        if (this->status = WL_CONNECTED)
        {
            break;
        }
    }
    WiFi.setDNS(DNS_IP);

    this->server.begin();
    delay(10000);
    this->printWifiStatus();
    return (this->status);
}

void WifiHelper::stopValidation()
{
    this->keepValidating = false;
}