using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

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

        public EventViewerControl CreateControl()
        {
            RepopulateAllRacerData();
            return new EventViewerControl { DataContext = _vm };
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
            bool hasHandicapData = _vm.IsHandicapMode;
            var dialog = new ExportOptionsDialog(hasHandicapData)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true) return;

            new EventHtmlExporter(this._event, this._vm.AllRacerData.ToList()).Export(dialog.Result!);
        }

        internal void LoadHandicaps()
        {
            // Gather all historical laps (excluding the current event) for each racer
            var eventsToLoad = new HashSet<long>();
            var racerHistoricalLaps = new Dictionary<long, List<DbLap>>();

            foreach (var racer in _vm.AllRacerData)
            {
                var allLaps = _dbLapsTable.getAllLapsForRacer(racer.PersonId)
                    .Where(l => l.EventId != _event.ID && l.LapCount > 0)
                    .ToList();
                racerHistoricalLaps[racer.PersonId] = allLaps;
                foreach (var lap in allLaps)
                    eventsToLoad.Add(lap.EventId);
            }

            var eventsById = _dbEventsTable.getEventsByIds(eventsToLoad.ToList())
                .ToDictionary(e => (long)e.ID);

            // Find the best pace (ms/m) for each racer using laps >= 400m
            var racerHistories = new List<(RacerData Racer, double BestPace, int BestLapLength)>();

            foreach (var racer in _vm.AllRacerData)
            {
                double bestPace = double.MaxValue;
                int bestLapLength = 0;

                foreach (var lap in racerHistoricalLaps[racer.PersonId])
                {
                    int effectiveLength = lap.LapLength
                        ?? (eventsById.TryGetValue(lap.EventId, out var ev) ? ev.LapLength : 0);
                    if (effectiveLength < 400) continue;

                    double pace = (double)lap.LapTime / effectiveLength;
                    if (pace < bestPace)
                    {
                        bestPace = pace;
                        bestLapLength = effectiveLength;
                    }
                }

                if (bestPace < double.MaxValue)
                    racerHistories.Add((racer, bestPace, bestLapLength));
                else
                    racer.HandicapPerLapMs = 0;
            }

            if (!racerHistories.Any()) return;

            // Reference = participant with the fastest (lowest) pace
            var reference = racerHistories.MinBy(r => r.BestPace)!;
            int todayLapLength = _event.LapLength;

            foreach (var (racer, bestPace, bestLapLength) in racerHistories)
            {
                double shortness = Math.Max(0, reference.BestLapLength - bestLapLength) / 100.0;
                double penalty = Math.Min(0.12, shortness * 0.02);
                double adjustedPace = bestPace * (1 + penalty);
                long handicap = (long)Math.Max(0, (adjustedPace - reference.BestPace) * todayLapLength);
                racer.HandicapPerLapMs = handicap;
            }
        }

        internal void ExecuteOnPersonChanged(RacerData racer)
        {
            if (racer == null) return;

            // This function relies on the racer instance having its PersonId value mutated so that
            // PersonId has a different value than Person.PersonId
            List<DbLap> lapsForRacer = _dbLapsTable.getEventLapsForRacer(racer.Person.PersonId, _event.ID);

            foreach (DbLap lap in lapsForRacer)
            {
                _dbLapsTable.updateLapPerson(lap.LapId, racer.PersonId);
            }

            RepopulateAllRacerData();
        }
    }
}

