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

        public IReadOnlyList<LapLengthStats> LapLengthBreakdown { get; private set; } = Array.Empty<LapLengthStats>();

        public bool HasMultipleLapLengths => LapLengthBreakdown.Count > 0;

        public void SetEventDefaults(Dictionary<long, int> eventLapLengths)
        {
            int EffectiveLength(DbLap l) =>
                l.LapLength ?? (eventLapLengths.TryGetValue(l.EventId, out var def) ? def : 0);

            // Real laps (excludes start marker): used for count and distance
            var realLaps = Laps
                .Where(l => l.LapCount > 0)
                .Select(l => (Lap: l, Length: EffectiveLength(l)))
                .Where(x => x.Length > 0)
                .ToList();

            // Timed real laps: used for best/avg/total time
            var timedLaps = realLaps.Where(x => x.Lap.LapTime > 0).ToList();

            var distinctLengths = timedLaps
                .Select(x => x.Length)
                .Distinct()
                .OrderByDescending(l => l)
                .Take(3)
                .ToList();

            if (distinctLengths.Count <= 1)
            {
                LapLengthBreakdown = Array.Empty<LapLengthStats>();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LapLengthBreakdown)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasMultipleLapLengths)));
                return;
            }

            LapLengthBreakdown = distinctLengths.Select(length =>
            {
                var real  = realLaps .Where(x => x.Length == length).ToList();
                var timed = timedLaps.Where(x => x.Length == length).ToList();
                return new LapLengthStats
                {
                    LengthM = length,
                    Count   = real.Count,
                    TotalMs = (ulong)timed.Aggregate<(DbLap Lap, int Length), decimal>(0, (s, x) => s + x.Lap.LapTime),
                    BestMs  = timed.Min(x => x.Lap.LapTime),
                    AvgMs   = (ulong)(timed.Aggregate<(DbLap Lap, int Length), decimal>(0, (s, x) => s + x.Lap.LapTime) / timed.Count)
                };
            }).ToList();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LapLengthBreakdown)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasMultipleLapLengths)));
        }
    }
}
