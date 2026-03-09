using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TrailMeisterDb;
using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    internal class PersonalLogHtmlExporter : RacerDataHtmlExporter
    {
        private readonly DbPerson _person;
        private readonly List<RacerEventRow> _eventRows;

        internal PersonalLogHtmlExporter(DbPerson person, List<RacerEventRow> eventRows)
        {
            _person = person;
            _eventRows = eventRows;
        }

        internal override void Export()
        {
            string xsltPath = Path.Combine(AppContext.BaseDirectory, "Windows", "PersonalLog", "Log.xslt");
            string dir = Path.Combine(OutputBaseDirectory, "PersonalLogs");
            Directory.CreateDirectory(dir);

            string fileName = $"{_person.FirstName}_{_person.LastName}.html";
            TransformXmlToHtml(BuildXml(), xsltPath, Path.Combine(dir, fileName));
            OpenOutputFolder(dir);
        }

        private XDocument BuildXml()
        {
            return new XDocument(
                new XElement("PersonalLog",
                    new XElement("FirstName", _person.FirstName),
                    new XElement("LastName", _person.LastName),
                    new XElement("NickName", _person.NickName),
                    BuildSummariesElement(),
                    new XElement("Events",
                        _eventRows
                            .Where(r => r.Event != null && r.RacerData.Laps.Count > 0)
                            .Select(r => new XElement("Event",
                                new XElement("EventName", r.Event!.EventName),
                                new XElement("EventDate", r.Event.EventDate.ToString("yyyy-MM-dd")),
                                new XElement("Laps",
                                    r.RacerData.Laps.Select(lap => new XElement("Lap",
                                        new XElement("LapNumber", lap.LapCount),
                                        new XElement("LapTime", lap.LapTime),
                                        new XElement("LapLengthMetres", lap.LapLength ?? r.Event.LapLength)
                                    ))
                                )
                            ))
                    )
                )
            );
        }

        private XElement BuildSummariesElement()
        {
            var groups = new List<(string Label, List<RacerEventRow> Rows)>
            {
                ("All Time", _eventRows.Where(r => r.Event != null && r.RacerData.Laps.Count > 0).ToList())
            };

            var datedRows = _eventRows.Where(r => r.Event != null && r.RacerData.Laps.Count > 0).ToList();
            if (datedRows.Any())
            {
                DateTime today = DateTime.Today;
                int sm = TrailMeisterDb.AppSettings.Current.SeasonStartMonth;
                int currentSeasonYear = today.Month >= sm ? today.Year : today.Year - 1;
                int earliestSeasonYear = datedRows.Min(r =>
                    r.Event!.EventDate.Month >= sm ? r.Event.EventDate.Year : r.Event.EventDate.Year - 1);

                for (int year = currentSeasonYear; year >= earliestSeasonYear; year--)
                {
                    DateTime start = new DateTime(year, sm, 1);
                    DateTime end = new DateTime(year + 1, sm, 1);
                    var seasonRows = datedRows.Where(r => r.Event!.EventDate >= start && r.Event.EventDate < end).ToList();
                    if (!seasonRows.Any()) continue;

                    string label = year == currentSeasonYear ? "This Season" : $"{year}/{(year + 1) % 100:D2}";
                    groups.Add((label, seasonRows));
                }
            }

            return new XElement("Summaries",
                groups.Select(g =>
                {
                    int lapCount = 0;
                    long totalTimeMs = 0;
                    long totalDistanceMetres = 0;
                    long bestLapMs = long.MaxValue;

                    foreach (var row in g.Rows)
                    {
                        int eventDefault = row.Event?.LapLength ?? 0;
                        foreach (var lap in row.RacerData.Laps)
                        {
                            lapCount++;
                            totalTimeMs += (long)lap.LapTime;
                            totalDistanceMetres += lap.LapLength ?? eventDefault;
                            if ((long)lap.LapTime < bestLapMs) bestLapMs = (long)lap.LapTime;
                        }
                    }

                    if (bestLapMs == long.MaxValue) bestLapMs = 0;
                    long avgLapMs = lapCount > 0 ? totalTimeMs / lapCount : 0;

                    return new XElement("Summary",
                        new XElement("Label", g.Label),
                        new XElement("LapCount", lapCount),
                        new XElement("TotalDistanceMetres", totalDistanceMetres),
                        new XElement("TotalTimeMs", totalTimeMs),
                        new XElement("BestLapMs", bestLapMs),
                        new XElement("AvgLapMs", avgLapMs)
                    );
                })
            );
        }
    }
}
