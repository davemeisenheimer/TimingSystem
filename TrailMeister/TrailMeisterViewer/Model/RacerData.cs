using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrailMeisterDb;
using TrailMeisterUtilities.Converters;

namespace TrailMeisterViewer.Model
{
    public class RacerData : INotifyPropertyChanged
    {
        private readonly Laps2TimeConverter laps2TimeConverter = new Laps2TimeConverter();
        internal RacerData(DbPerson person, List<DbLap> eventLaps)
        {
            this.Person = person;
            this.Laps = eventLaps;
            this.PersonId = person.PersonId;

            foreach (var lap in eventLaps)
                lap.PropertyChanged += OnLapPropertyChanged;
        }

        private void OnLapPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DbLap.LapLength))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Laps)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Laps is a collection that can store laps for multiple contexts:
        //  e.g. could be for a given event, a given season, or all time
        public List<DbLap> Laps { get; set; }

        // Account for zero-based lap records i.e. the start is considered lap 0
        public int LapCount { get { return this.Laps.Count - this.Laps.Select(lap => lap.EventId).Distinct().Count(); } }

        public DbPerson Person { get; set; }

        public long PersonId { get; set; }

        private long _handicapPerLapMs;
        public long HandicapPerLapMs
        {
            get => _handicapPerLapMs;
            set
            {
                if (_handicapPerLapMs != value)
                {
                    _handicapPerLapMs = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HandicapPerLapMs)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdjustedBestLapMs)));
                }
            }
        }

        // Used for sorting when handicap mode is active
        public long AdjustedBestLapMs
        {
            get
            {
                var validLaps = Laps.Where(l => l.LapTime > 0 && l.LapCount > 0).ToList();
                if (!validLaps.Any()) return long.MaxValue;
                return Math.Max(0, (long)validLaps.Min(l => l.LapTime) - HandicapPerLapMs);
            }
        }

        public string BestLap
        {
            get
            {
                return (String)laps2TimeConverter.Convert(new object[] { TimeConversionType.BestLap, Laps }, typeof(object), new object(), CultureInfo.CurrentCulture);
            }
        }

        public string TotalTime
        {
            get
            {
                return (String)laps2TimeConverter.Convert(new object[] { TimeConversionType.TotalTime, Laps }, typeof(object), new object(), CultureInfo.CurrentCulture);
            }
        }

        public string AverageLap
        {
            get
            {
                return (String)laps2TimeConverter.Convert(new object[] { TimeConversionType.AverageLap, Laps }, typeof(object), new object(), CultureInfo.CurrentCulture);
            }
        }
    }
}
