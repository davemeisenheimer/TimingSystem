#include "TagTracker.h"
#include "Config.h"
#include <cstring>

int TagTracker::findOrCreate(const byte* epc, byte epcLen)
{
    for (int i = 0; i < foundCount; i++)
    {
        if (tags[i].epcBytes == epcLen &&
            memcmp(tags[i].epc, epc, epcLen) == 0)
        {
            return i;
        }
    }

    if (foundCount >= SUPPORTED_TAG_COUNT)
        return -1;

    Tag& t = tags[foundCount];
    memcpy(t.epc, epc, epcLen);
    t.epcBytes = epcLen;
    t.state = LINE_DATA_DETECT;
    t.rssi = -32000;
    t.nextStateTime = 0;

    foundCount++;
    return foundCount - 1;
}

void TagTracker::onTagRead(const Tag& tag)
{
    int idx = findOrCreate(tag.epc, tag.epcBytes);
    if (idx < 0)
        return;

    Tag& trackedTag = tags[idx];
    
    switch(trackedTag.state)
    {
        case LINE_DATA_DETECT:
            trackedTag.state = LINE_DATA_GATHERING;
            trackedTag.rssi = tag.rssi;
            trackedTag.timestamp = tag.timestamp;
            trackedTag.peakTimestamp = tag.timestamp;
            trackedTag.nextStateTime = millis() + TAG_STATE_DELAY_FROM_GATHERING;
            pendingCount++;
            break;

        case LINE_DATA_GATHERING:
            if (tag.rssi >= trackedTag.rssi)
            {
                trackedTag.rssi = tag.rssi;
                trackedTag.timestamp = tag.timestamp;
                trackedTag.peakTimestamp = tag.timestamp;
                trackedTag.nextStateTime = millis() + TAG_STATE_DELAY_FROM_GATHERING;
            } else {
                // Force this tag to get sent if we are seeing a drop-off in signal strength
                // Will be handled in shouldSend on next check.
                // Should really create a state machine for this stuff.
                trackedTag.nextStateTime = millis() - TAG_STATE_DELAY_FROM_GATHERING;
            }
            break;

        case LINE_DATA_SENT:
            // Tag is still loitering in the read zone after being sent. Delay next lap detection unti
            // we have proof they are out on the trail.
            trackedTag.nextStateTime = millis() + TAG_STATE_DELAY_FROM_SENT;
            break;
    }
}

// void TagTracker::onTagRead(const Tag& tag)
// {
//     int idx = findOrCreate(tag.epc, tag.epcBytes);
//     if (idx < 0)
//         return;

//     Tag& trackedTag = tags[idx];

//     bool stronger = tag.rssi >= trackedTag.rssi;
//     bool readyToDetect = millis() > trackedTag.nextStateTime &&
//                         trackedTag.state == LINE_DATA_DETECT;

//     if (stronger && (trackedTag.state == LINE_DATA_GATHERING || readyToDetect))
//     {
//         if (readyToDetect)
//             pendingCount++;

//         trackedTag.state = LINE_DATA_GATHERING;
//         trackedTag.rssi = tag.rssi;
//         trackedTag.timestamp = tag.timestamp;
//         trackedTag.nextStateTime = millis() + TAG_STATE_DELAY_FROM_GATHERING;
//     }
// }

bool TagTracker::shouldSend(Tag& tag)
{
    if (millis() > tag.nextStateTime &&
        tag.state == LINE_DATA_GATHERING)
    {
        tag.state = LINE_DATA_SENT;
        return true;
    }

    if (millis() > tag.nextStateTime &&
        tag.state == LINE_DATA_SENT)
    {
        tag.state = LINE_DATA_DETECT;
        // Once we are in the DETECT state, we can detect a new lap immediately.
        tag.nextStateTime = millis(); 
        pendingCount--;
    }

    return false;
}

bool TagTracker::hasPending() const
{
    return pendingCount > 0;
}

Tag* TagTracker::getPendingTag()
{
    for (int i = 0; i < foundCount; i++)
    {
        if (shouldSend(tags[i]))
            return &tags[i];
    }
    return nullptr;
}

void TagTracker::markSent(Tag* tag)
{
    tag->state = LINE_DATA_SENT;
    tag->rssi = -32000;
    tag->nextStateTime = millis();
}
