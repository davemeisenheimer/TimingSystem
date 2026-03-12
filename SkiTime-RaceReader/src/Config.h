#pragma once

// Comment or uncomment
// #define USE_TEST_DATA

// #define DO_SEND_DEBUG_MSG
#define DO_SERIAL true

#define SUPPORTED_TAG_COUNT 32
#define TAG_MAX_SEND_COUNT 6

#define SOCKET_SERVER_IP    "192.168.0.194"
#define SOCKET_SERVER_PORT  13000

#define SOCKET_LOCAL_PORT   13001
#define WIFI_CONTROL_PORT   80

#define RFID_ANTENNA_POWER_MIN  500   // 5 dBm — minimum safe power for idle/participant-list scanning
#define RFID_ANTENNA_POWER_MAX  2700  // 27 dBm — maximum power for race use

#define TAG_STATE_DELAY_FROM_GATHERING      350
#define TAG_STATE_DELAY_FROM_SENT           30000
#define LAP_IGNORE_MS       30000