# TimingSystem

This repository contains 2 separate projects that are desiged to work together:

## 1. SkiTime-RaceReader
- An Arduino project designed to work with the Arduino ESP32 Nano, with the M7E-Nano shield
- sketch that reads tag data, finds the strongest read in a batch, and sends data over wifi socket

## 2. TrailMeister
- WPF/C# Race software
- Use this software to:
  1. Add people to database
  2. Associate people with a tag for a given event
  3. Create a training event and display results while in progress
  4. Manage details of events after they're done
- This project was originally created to work with the M6e Nano over USB (rather than wifi), and still can do that
