#ifndef NANOREADER_H_
#define NANOREADER_H_

#define BUZZER1 10
//#define BUZZER1 0 //For testing silently
#define BUZZER2 9

class NanoReader
{
public:
    static void init()
    {
        pinMode(BUZZER1, OUTPUT);
        pinMode(BUZZER2, OUTPUT);
        digitalWrite(BUZZER2, LOW); // Pull half the buzzer to ground and drive the other half.

        lowBeep(); // Indicate no tag found
    }
    static void lowBeep()
    {
        tone(BUZZER1, 130, 150); // Low C
        // delay(150);
    }

    static void highBeep()
    {
        tone(BUZZER1, 2093, 150); // High C
        // delay(150);
    }
};

#endif