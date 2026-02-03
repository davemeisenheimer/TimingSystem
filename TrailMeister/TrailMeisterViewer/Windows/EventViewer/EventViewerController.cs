using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

using TrailMeisterDb;
using TrailMeisterViewer.Model;
using System.Diagnostics;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public class EventViewerController
    {
        DbLapsTable _dbLapsTable = new DbLapsTable();
        DbEventsTable _dbEventsTable = new DbEventsTable();
        DbPeopleTable _dbPeopleTable = new DbPeopleTable();
        DbEvent _event;
        EventViewerVM _vm;

        internal EventViewerController(DbEvent dbEvent)
        {
            _event = dbEvent;
            _event.PropertyChanged += OnEventPropertyChanged;
            _vm = new EventViewerVM(this, dbEvent);
            init();
        }

        private void init()
        {
            List<DbPerson> people = _dbPeopleTable.getPeople();

            foreach (DbPerson person in people)
            {
                this._vm.AllPeople.Add(person);
            }
        }

        private void OnEventPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this._dbEventsTable.updateEvent(_event.ID, _event.EventName, _event.LapLength, _event.EventFinished);
            RepopulateAllRacerData();
        }

        public void ShowWindow()
        {
            RepopulateAllRacerData();

            var window = new EventViewer
            {
                DataContext = _vm,
                Owner = null
            };

            window.ShowDialog();
        }

        private void RepopulateAllRacerData()
        {
            _vm.AllRacerData.Clear();
            List<DbLap> eventLaps = _dbLapsTable.getEventLapsForEvent(_event.ID);

            List<long> racerIds = eventLaps
                .Select(x => x.PersonId)
                .Distinct()
                .ToList();

            foreach (int racerId in racerIds)
            {
                DbPerson? racer = this._dbPeopleTable.getPerson(racerId);
                if (racer != null)
                {
                    _vm.AllRacerData.Add(new
                        RacerData(
                            racer,
                            eventLaps.Where(x => x.PersonId == racerId && x.LapCount > 0)
                                .ToList()
                        )
                    );
                }
            }
        }

        internal void ExportHtml()
        {
            RacerDataXmlSerializer.ExportEventToHtml(this._event, this._vm.AllRacerData.ToList());
        }

        internal void ExecuteOnPersonChanged(RacerData racer)
        {
            if (racer == null) return;

            // This function relies on the racer instance having its PersonId value mutated so that
            // PersonId has a different value than Person.PersonId
            List<DbLap> lapsForRacer = _dbLapsTable.getAllLapsForRacer(racer.Person.PersonId);

            foreach (DbLap lap in lapsForRacer)
            {
                _dbLapsTable.updateLapPerson(lap.LapId, racer.PersonId);
            }

            RepopulateAllRacerData();
        }
    }
}

