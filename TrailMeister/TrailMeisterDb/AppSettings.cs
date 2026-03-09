using System;
using System.IO;

namespace TrailMeisterDb
{
    public class AppSettings
    {
        /// <summary>
        /// The single shared instance. Populated by AppSettingsService.Load() at startup;
        /// defaults are used if no settings file exists yet.
        /// </summary>
        public static AppSettings Current { get; set; } = new AppSettings();

        // ── Export ────────────────────────────────────────────────────────────
        public string ExportOutputDirectory { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrailMeisterExports");

        // ── Database ──────────────────────────────────────────────────────────
        public string DbServer   { get; set; } = "localhost";
        public string DbName     { get; set; } = "skimeister";
        public string DbUserId   { get; set; } = "skimeister";
        public string DbPassword { get; set; } = "P@ssw0rd";

        // ── Handicap algorithm ────────────────────────────────────────────────
        /// <summary>Laps shorter than this (in metres) are excluded from handicap calculations.</summary>
        public int    HandicapMinLapLengthM       { get; set; } = 400;
        /// <summary>Pace penalty applied per 100 m by which a racer's best lap was shorter than the reference racer's.</summary>
        public double HandicapPenaltyPerHundredM  { get; set; } = 0.02;
        /// <summary>Maximum total pace penalty (as a fraction, e.g. 0.12 = 12%).</summary>
        public double HandicapMaxPenalty          { get; set; } = 0.12;

        // ── Season ────────────────────────────────────────────────────────────
        /// <summary>Month number (1–12) on which a new season begins.</summary>
        public int SeasonStartMonth { get; set; } = 9; // September

        // ── Arduino / RFID reader ─────────────────────────────────────────────
        public string ArduinoIpAddress { get; set; } = "192.168.0.94";
        public int    ArduinoPort      { get; set; } = 13001;
    }
}
