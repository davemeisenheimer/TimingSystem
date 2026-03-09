using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TrailMeisterDb;
using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.EventViewer
{
    internal class EventHtmlExporter : RacerDataHtmlExporter
    {
        private readonly DbEvent _event;
        private readonly List<RacerData> _racers;

        internal EventHtmlExporter(DbEvent ev, List<RacerData> racers)
        {
            _event = ev;
            _racers = racers;
        }

        // Satisfies the abstract base; uses defaults
        internal override void Export() =>
            Export(new ExportOptions { IncludeRawResults = true, IncludeRanking = true });

        internal void Export(ExportOptions opts)
        {
            string xsltPath = Path.Combine(AppContext.BaseDirectory, "Windows", "EventViewer", "Event.xslt");
            string dir = Path.Combine(OutputBaseDirectory, _event.EventName);
            Directory.CreateDirectory(dir);

            var allSlices = _racers.Select(ToSlice).ToList();

            // All-racers summary file
            TransformXmlToHtml(BuildXml(allSlices, opts), xsltPath, Path.Combine(dir, "AllRacers.html"));

            // Per-racer individual files (no ranking or pruning — single racer only)
            var perRacerOpts = new ExportOptions
            {
                IncludeRawResults = opts.IncludeRawResults,
                IncludeHandicapResults = opts.IncludeHandicapResults,
                SortBy = opts.SortBy,
                IncludeRanking = false,
            };
            foreach (var racer in _racers)
            {
                var slice = ToSlice(racer);
                string fileName = $"{racer.Person.FirstName}_{racer.Person.LastName} ({racer.Person.NickName}).html";
                TransformXmlToHtml(BuildXml(new List<RacerSlice> { slice }, perRacerOpts),
                    xsltPath, Path.Combine(dir, fileName));
            }

            OpenOutputFolder(dir);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static RacerSlice ToSlice(RacerData r) =>
            new RacerSlice(r.Person, r.HandicapPerLapMs, r.Laps);

        private XDocument BuildXml(List<RacerSlice> slices, ExportOptions opts)
        {
            bool hasPruning = opts.PruneEarlyLaps || opts.PruneLastLaps;
            var resultSets = new List<XElement>();

            // --- Unpruned sets ---
            if (opts.IncludeRawResults || hasPruning)
            {
                string label = hasPruning ? "Full Results" : "Results";
                resultSets.Add(BuildResultSet(label, slices, opts.IncludeRanking, opts.SortBy, isHandicap: false));
            }

            if (opts.IncludeHandicapResults)
            {
                string label = hasPruning ? "Handicapped Full Results" : "Handicapped Results";
                resultSets.Add(BuildResultSet(label, slices, opts.IncludeRanking, opts.SortBy, isHandicap: true));
            }

            // --- Pruned sets ---
            if (hasPruning)
            {
                var pruned = ApplyPruning(slices, pruneEarly: opts.PruneEarlyLaps);
                int minLaps = pruned.Any() ? pruned.First().Laps.Count : 0;
                string pruneDesc = opts.PruneEarlyLaps
                    ? $"last {minLaps} laps per racer"
                    : $"first {minLaps} laps per racer";

                if (opts.IncludeRawResults || hasPruning)
                    resultSets.Add(BuildResultSet($"Pruned Results ({pruneDesc})", pruned,
                        opts.IncludeRanking, opts.SortBy, isHandicap: false));

                if (opts.IncludeHandicapResults)
                    resultSets.Add(BuildResultSet($"Handicapped Pruned Results ({pruneDesc})", pruned,
                        opts.IncludeRanking, opts.SortBy, isHandicap: true));
            }

            return new XDocument(
                new XElement("Event",
                    new XElement("EventName", _event.EventName),
                    new XElement("EventDate", _event.EventDate.ToString("yyyy-MM-dd")),
                    new XElement("ResultSets", resultSets)
                )
            );
        }

        private static XElement BuildResultSet(
            string label, List<RacerSlice> slices,
            bool includeRanking, ResultSortField sortBy, bool isHandicap)
        {
            // Compute per-racer metrics
            var scored = slices.Select(s =>
            {
                var laps = s.Laps.Where(l => l.LapCount > 0 && l.LapTime > 0).ToList();
                long handicapOffset = isHandicap ? s.HandicapPerLapMs : 0L;

                long totalMs = 0;
                long bestMs = long.MaxValue;
                foreach (var l in laps)
                {
                    totalMs += (long)l.LapTime;
                    if ((long)l.LapTime < bestMs) bestMs = (long)l.LapTime;
                }
                if (bestMs == long.MaxValue) bestMs = 0;

                long adjTotal = Math.Max(0, totalMs - handicapOffset * laps.Count);
                long adjBest  = Math.Max(0, bestMs  - handicapOffset);
                long adjAvg   = laps.Count > 0 ? adjTotal / laps.Count : 0;

                return (Slice: s, Laps: laps, Total: adjTotal, Best: adjBest, Avg: adjAvg);
            }).ToList();

            // Sort by the chosen field
            var sorted = sortBy switch
            {
                ResultSortField.BestLap    => scored.OrderBy(x => x.Best).ToList(),
                ResultSortField.AverageLap => scored.OrderBy(x => x.Avg).ToList(),
                ResultSortField.TotalLaps  => scored.OrderByDescending(x => x.Laps.Count).ThenBy(x => x.Total).ToList(),
                _                          => scored.OrderBy(x => x.Total).ToList(),
            };

            // Build XML
            int rank = 1;
            return new XElement("ResultSet",
                new XAttribute("Label", label),
                new XAttribute("IncludeRanking", includeRanking ? "true" : "false"),
                new XElement("Racers",
                    sorted.Select(x => new XElement("Racer",
                        new XElement("FirstName", x.Slice.Person.FirstName),
                        new XElement("LastName", x.Slice.Person.LastName),
                        new XElement("NickName", x.Slice.Person.NickName ?? string.Empty),
                        new XElement("Association", x.Slice.Person.Association ?? string.Empty),
                        new XElement("Rank", rank++),
                        new XElement("LapCount", x.Laps.Count),
                        new XElement("TotalTimeMs", x.Total),
                        new XElement("BestLapMs", x.Best),
                        new XElement("AvgLapMs", x.Avg),
                        new XElement("Laps",
                            x.Laps.Select(l => new XElement("Lap",
                                new XElement("LapNumber", l.LapCount),
                                new XElement("LapTimeMs", (long)l.LapTime)
                            ))
                        )
                    ))
                )
            );
        }

        /// <summary>
        /// Returns new slices whose Laps list contains only qualifying laps (LapCount &gt; 0),
        /// trimmed so every racer has the same count (the minimum across all racers).
        /// pruneEarly=true → keep the LAST N laps; pruneEarly=false → keep the FIRST N laps.
        /// </summary>
        private static List<RacerSlice> ApplyPruning(List<RacerSlice> slices, bool pruneEarly)
        {
            if (!slices.Any()) return slices;

            int minLaps = slices.Min(s => s.Laps.Count(l => l.LapCount > 0));

            return slices.Select(s =>
            {
                var qualifying = s.Laps.Where(l => l.LapCount > 0).ToList();
                var kept = pruneEarly
                    ? qualifying.Skip(qualifying.Count - minLaps).ToList()
                    : qualifying.Take(minLaps).ToList();
                return new RacerSlice(s.Person, s.HandicapPerLapMs, kept);
            }).ToList();
        }

        private class RacerSlice
        {
            public RacerSlice(DbPerson person, long handicapPerLapMs, List<DbLap> laps)
            {
                Person = person;
                HandicapPerLapMs = handicapPerLapMs;
                Laps = laps;
            }

            public DbPerson Person { get; }
            public long HandicapPerLapMs { get; }
            public List<DbLap> Laps { get; }
        }
    }
}
