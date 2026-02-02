#include "WifiHelper.h"
#include "Config.h"

const IPAddress GATEWAY_IP(192, 168, 0, 1);
const IPAddress MY_IP(192, 168, 0, MY_IP_OCTET);
const IPAddress DNS_IP(206, 248, 154, 22);
const IPAddress SUBNET_IP(255, 255, 255, 0);

WifiHelper::WifiHelper(SocketClient *socketClient) : 
    socketClient(socketClient),
    lastWdogCheckSecs(0),
    currentSecs(0),
    wifiRestarts(0),
    serverRestarts(0),
    lastCharWifiMs(0),
    lastWifiMsgSecs(0),
    status(WL_IDLE_STATUS),
    serverStatus(0),
    server(WIFI_CONTROL_PORT),
    keepValidating(true)
{
}

void WifiHelper::init()
{
    char ssid[] = SECRET_SSID;
    char pass[] = SECRET_PASS;

    Serial.println("\n[WiFi] Initializing");

    WiFi.mode(WIFI_STA);
    WiFi.disconnect(true);
    delay(200);

    // Static IP configuration
    if (!WiFi.config(MY_IP, GATEWAY_IP, SUBNET_IP, DNS_IP))
    {
        Serial.println("[WiFi] Static IP config failed");
    }

    status = WL_IDLE_STATUS;

    Serial.println("[WiFi] Connecting to " + String(ssid));

    WiFi.begin(ssid, pass);

    unsigned long startAttempt = millis();
    const unsigned long timeoutMs = 15000;

    while (WiFi.status() != WL_CONNECTED &&
           millis() - startAttempt < timeoutMs)
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println("Checking WiFi status...");

    if (WiFi.status() == WL_CONNECTED)
    {
        status = WL_CONNECTED;
        Serial.println("[WiFi] Connected");
        printWifiStatus();

        server.begin();
        socketClient->sendDebugMessage("[WiFi] Server started");
    }
    else
    {
        Serial.println("[WiFi] Connection failed – watchdog will retry");
        status = WiFi.status();
    }

    lastWifiMsgSecs = millis() / 1000;
}

unsigned long WifiHelper::getWifiEpoch()
{
    time_t now;
    time(&now);

    if (now < 100000) {   // time not synced yet
        this->invalidTimeSyncs++;
        if (DO_SERIAL) socketClient->sendDebugMessage("NTP not synced");
        return 0;
    }

    return (unsigned long)now;
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
        if (DO_SERIAL) socketClient->sendDebugMessage("Wait too long for next client character");
    }
}

void WifiHelper::printWifiStatus()
{
    // print the SSID of the network you're attached to:
    if (DO_SERIAL) socketClient->sendDebugMessage("SSID: " + WiFi.SSID());

    // print your board's IP address:
    IPAddress ip = WiFi.localIP();
    if (DO_SERIAL) socketClient->sendDebugMessage("IP Address: " + ip.toString());

    // print the received signal strength:
    long rssi = WiFi.RSSI();
    if (DO_SERIAL) socketClient->sendDebugMessage("RSSI:" + String(rssi) + " dBm");
    // print where to go in a browser:
    if (DO_SERIAL) socketClient->sendDebugMessage("IP: " + ip.toString());
}

void WifiHelper::validateWifiConnection()
{
    if (!keepValidating)
        return;

    unsigned long nowSecs = millis() / 1000;

    // Run watchdog once every 30 seconds
    if ((nowSecs / 30) == (lastWdogCheckSecs / 30))
        return;

    lastWdogCheckSecs = nowSecs;
    if (DO_SERIAL) socketClient->sendDebugMessage("."); // heartbeat

    // Check WiFi connection first (authoritative on ESP32)
    if (WiFi.status() != WL_CONNECTED)
    {
        if (DO_SERIAL) socketClient->sendDebugMessage("\nWiFi disconnected – restarting WiFi");
        restartWifi();
        return;
    }

    // Check for client activity
    WiFiClient client = server.available();
    if (client)
    {
        // Client connected – mark activity
        lastWifiMsgSecs = nowSecs;
        return;
    }

    // No traffic watchdog
    if ((nowSecs - lastWifiMsgSecs) > WATCHDOG_NO_MSG_SECS)
    {
        if (DO_SERIAL) socketClient->sendDebugMessage("\nNo WiFi traffic – restarting WiFi");
        restartWifi();
    }
}

int WifiHelper::restartWifi()
{
    char ssid[] = SECRET_SSID;
    char pass[] = SECRET_PASS;

    if (DO_SERIAL) socketClient->sendDebugMessage("Restarting WiFi");

    wifiRestarts++;

    // Fully reset WiFi subsystem
    WiFi.disconnect(true);
    WiFi.mode(WIFI_OFF);
    delay(500);
    WiFi.mode(WIFI_STA);

    status = WL_IDLE_STATUS;

    // Attempt reconnect (max 5 tries)
    for (int attempt = 1; attempt <= 5; attempt++)
    {
        if (DO_SERIAL) socketClient->sendDebugMessage("WiFi connect attempt " + String(attempt));

        WiFi.begin(ssid, pass);

        unsigned long start = millis();
        while (millis() - start < 10000)
        {
            if (WiFi.status() == WL_CONNECTED)
            {
                status = WL_CONNECTED;
                if (DO_SERIAL) socketClient->sendDebugMessage("WiFi reconnected");

                server.begin();
                lastWifiMsgSecs = millis() / 1000;
                printWifiStatus();
                return status;
            }
            delay(250);
        }
    }

    if (DO_SERIAL) socketClient->sendDebugMessage("WiFi restart failed");
    return status;
}

void WifiHelper::stopValidation()
{
    this->keepValidating = false;
}