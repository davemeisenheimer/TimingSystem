using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TrailMeisterDb;

namespace TrailMeisterUtilities.Converters
{
    /// <summary>
    /// Formats a lap time column, optionally applying a per-lap handicap offset.
    /// Values: [TimeConversionType conversionType, List<DbLap> eventLaps, long handicapPerLapMs, bool isHandicapMode]
    /// </summary>
    public class HandicapAwareTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 ||
                values[0] is not TimeConversionType conversionType ||
                values[1] is not List<DbLap> eventLaps ||
                values[3] is not bool isHandicapMode)
            {
                return string.Empty;
            }

            long handicapPerLapMs = values[2] is long h ? h : System.Convert.ToInt64(values[2] ?? 0L);

            var laps = eventLaps.Where(l => l.LapTime > 0 && l.LapCount > 0).ToList();
            if (!laps.Any()) return string.Empty;

            long rawMs;
            switch (conversionType)
            {
                case TimeConversionType.BestLap:
                    rawMs = (long)laps.Min(l => l.LapTime);
                    break;
                case TimeConversionType.AverageLap:
                    rawMs = (long)(laps.Aggregate<DbLap, decimal>(0, (s, l) => s + l.LapTime) / laps.Count);
                    break;
                case TimeConversionType.TotalTime:
                    rawMs = (long)laps.Aggregate<DbLap, decimal>(0, (s, l) => s + l.LapTime);
                    break;
                default:
                    rawMs = 0;
                    break;
            }

            long displayMs = rawMs;
            if (isHandicapMode)
            {
                long offset = conversionType == TimeConversionType.TotalTime
                    ? handicapPerLapMs * laps.Count
                    : handicapPerLapMs;
                displayMs = Math.Max(0, rawMs - offset);
            }

            var ms2Time = new Ms2TimeConverter();
            return ms2Time.Convert(new object[] { (ulong)displayMs, TimeConversionPrecision.ToTheHundredth }, targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
