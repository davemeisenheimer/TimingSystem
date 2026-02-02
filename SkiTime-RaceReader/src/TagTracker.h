#pragma once
#include <Arduino.h>
#include "Tag.h"

#define SUPPORTED_TAG_COUNT 32

class TagTracker
{
public:
    void onTagRead(const Tag& tag);

    bool hasPending() const;
    Tag* getPendingTag();
    void markSent(Tag* tag);

private:
    int findOrCreate(const byte* epc, byte epcLen);
    bool shouldSend(Tag& tag);

    Tag tags[SUPPORTED_TAG_COUNT];
    int foundCount = 0;
    int pendingCount = 0;
};