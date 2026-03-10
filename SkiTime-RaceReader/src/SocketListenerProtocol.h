#pragma once

enum class ListenerCommand
{
    SetAntennaGain,
    StopWifiValidation,
    StopListening,
    Reset,
    Unknown
};

// String tokens expected from client
#define CMD_SET_ANTENNA_GAIN "SetAntennaGain"
#define CMD_STOP_WIFI       "StopWifiValidation"
#define CMD_RESET           "reset"
#define CMD_START_READER    "StartReader"
#define CMD_STOP_READER     "StopReader"