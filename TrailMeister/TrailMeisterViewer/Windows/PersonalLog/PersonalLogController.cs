using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;

using TrailMeisterDb;

using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public class PersonalLogController
    {
        DbLapsTable _dbLapsTable = new DbLapsTable();
        DbEventsTable _dbEventsTable = new DbEventsTable();
        DbPeopleTable _dbPeopleTable = new DbPeopleTable();
        DbPerson _person;
        PersonalLogVM _vm;

        internal PersonalLogController(DbPerson dbPerson)
        {
            _person = dbPerson;
            _vm = new PersonalLogVM(this, dbPerson);
        }

        public PersonalLogControl CreateControl()
        {
            List<DbLap> personLaps = _dbLapsTable.getAllLapsForRacer(_person.PersonId);

            var eventIds = personLaps.Select(l => l.EventId).Distinct().ToList();
            var eventLookup = _dbEventsTable.getEventsByIds(eventIds)
                                            .ToDictionary(e => e.ID);

            var eventLapLengths = eventLookup.ToDictionary(kvp => (long)kvp.Key, kvp => kvp.Value.LapLength);

            _vm.Summaries = buildAllSummaries(eventLookup, personLaps, eventLapLengths);

            var eventRows = personLaps.GroupBy(lap => lap.EventId)
                .Select(group =>
                {
                    var ev = eventLookup.TryGetValue((uint)group.Key, out var e) ? e : null;
                    var rd = new RacerData(_person, group.Where(x => x.LapCount > 0).ToList());
                    rd.SetEventDefaults(eventLapLengths);
                    return new RacerEventRow { Event = ev, RacerData = rd };
                })
                .OrderByDescending(row => row.Event?.EventDate ?? DateTime.MinValue)
                .ToList();

            foreach (var row in eventRows)
            {
                _vm.AllEventRows.Add(row);
            }

            return new PersonalLogControl { DataContext = _vm };
        }

        private List<SeasonSummary> buildAllSummaries(Dictionary<uint, DbEvent> allEvents, List<DbLap> personLaps, Dictionary<long, int> eventLapLengths)
        {
            var allTimeData = new RacerData(_person, personLaps);
            allTimeData.SetEventDefaults(eventLapLengths);
            var summaries = new List<SeasonSummary>
            {
                new SeasonSummary { Label = "All Time", RacerData = allTimeData }
            };

            if (!allEvents.Any() || !personLaps.Any())
                return summaries;

            DateTime today = DateTime.Today;
            int sm = TrailMeisterDb.AppSettings.Current.SeasonStartMonth;
            int currentSeasonYear = today.Month >= sm ? today.Year : today.Year - 1;
            int earliestSeasonYear = allEvents.Values.Min(e =>
                e.EventDate.Month >= sm ? e.EventDate.Year : e.EventDate.Year - 1);

            for (int year = currentSeasonYear; year >= earliestSeasonYear; year--)
            {
                DateTime start = new DateTime(year, sm, 1);
                DateTime end = new DateTime(year + 1, sm, 1);

                var seasonEventIds = new HashSet<long>(
                    allEvents.Values
                        .Where(e => e.EventDate >= start && e.EventDate < end)
                        .Select(e => (long)e.ID));

                var seasonLaps = personLaps.Where(l => seasonEventIds.Contains(l.EventId)).ToList();
                if (!seasonLaps.Any()) continue;

                string label = year == currentSeasonYear
                    ? "This Season"
                    : $"{year}/{(year + 1) % 100:D2}";

                var seasonData = new RacerData(_person, seasonLaps);
                seasonData.SetEventDefaults(eventLapLengths);
                summaries.Add(new SeasonSummary { Label = label, RacerData = seasonData });
            }

            return summaries;
        }

        internal void ExportHtml()
        {
            new PersonalLogHtmlExporter(_person, _vm.AllEventRows.ToList()).Export();
        }
    }
}

