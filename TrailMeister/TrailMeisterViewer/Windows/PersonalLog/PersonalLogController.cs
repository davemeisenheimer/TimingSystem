using System.Windows;
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

        public void ShowWindow()
        {
            List<DbLap> personLaps = _dbLapsTable.getAllLapsForRacer(_person.PersonId);

            var eventIds = personLaps.Select(l => l.EventId).Distinct().ToList();
            var eventLookup = _dbEventsTable.getEventsByIds(eventIds)
                                            .ToDictionary(e => e.ID);


            _vm.RacerDataAll = new RacerData(_person, personLaps);
            _vm.RacerDataSeason = this.getRacerDataForCurrentSeason(eventLookup, personLaps);

            // Create the grouped rows
            var eventRows = personLaps.GroupBy(lap => lap.EventId)
                .Select(group => new RacerEventRow
                {
                    // Use the lookup to get the full object; fallback to null if not found
                    Event = eventLookup.TryGetValue((uint)group.Key, out var ev) ? ev : null,

                    // Pass only the laps belonging to THIS specific event group
                    RacerData = new RacerData(
                                    _person,
                                    group.Where(x => x.LapCount > 0).ToList()
                                )
                 })
                .OrderByDescending(row => row.Event?.EventDate ?? DateTime.MinValue)
                .ToList();

            foreach(var row in eventRows)
            {
                _vm.AllEventRows.Add(row);
            }

            //_vm.AllRacerData.Add(new
            //    RacerData(
            //        _person,
            //        personLaps.Where(x => x.LapCount > 0)
            //            .ToList()
            //    )
            //);

            var window = new PersonalLog
            {
                DataContext = _vm,
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
        }

        private RacerData getRacerDataForCurrentSeason(Dictionary<uint, DbEvent> allEvents, List<DbLap> personLaps)
        {
            DateTime today = DateTime.Today;
            int year = today.Month >= 9 ? today.Year : today.Year - 1;
            DateTime previousSeptember = new DateTime(year, 9, 1);

            var eventsThisSeason = allEvents.Values
                                            .Where(e => e.EventDate >= previousSeptember && e.EventDate <= today);

            var filteredEventIds = new HashSet<long>(eventsThisSeason.Select(e => (long)e.ID));

            var seasonLaps = personLaps
                .Where(lap => filteredEventIds.Contains(lap.EventId));

            return new RacerData(_person, seasonLaps.ToList());
        }

        internal void ExportHtml()
        {
            //RacerDataXmlSerializer.ExportEventToHtml(this._event, this._vm.AllRacerData.ToList());
        }
    }
}

