#pragma once
#include <Arduino.h>

enum LapState
{
    LINE_DATA_GATHERING,
    LINE_DATA_SENT,
    LINE_DATA_DETECT
};

struct Tag
{
    byte epc[64];
    byte epcBytes = 0;
    int rssi = -32000;
    unsigned long timestamp = 0;
    unsigned long peakTimestamp = 0;
    unsigned long nextStateTime = 0;
    LapState state = LINE_DATA_DETECT;
};