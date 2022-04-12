An Arduino Interface to the M6D-Nano

This is a sketch that is designed to acquire RFID tag reads from an M6E-Nano shield.  The sketch receives tag data, accesses multiple subsequent reads until a drop in RSSI is detected, then sends the read data over a socket to an app that is listening for this data.  The board that this was written for is a Arduino Wifi Uno Rev2.  The idea here is that a tag data can be sent wirelessly where wifi is available.

