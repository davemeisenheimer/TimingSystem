using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

using TrailMeisterDb;
using TrailMeisterViewer.Model;

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
        }

        private void OnEventPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this._dbEventsTable.updateEvent(_event.ID, _event.EventName, _event.LapLength, _event.EventFinished);
        }

        public void ShowWindow()
        {
            List<DbLap> eventLaps = _dbLapsTable.getEventLapsForEvent(_event.ID);

            List<long> racerIds = eventLaps
                .Select(x => x.PersonId)
                .Distinct()
                .ToList();

            foreach (int racerId in racerIds)
            {
                DbPerson racer = this._dbPeopleTable.getPerson(racerId);
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

            var window = new EventViewer
            {
                DataContext = _vm,
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
        }

        internal void ExportHtml()
        {
            RacerDataXmlSerializer.ExportEventToHtml(this._event, this._vm.AllRacerData.ToList());
        }
    }
}

