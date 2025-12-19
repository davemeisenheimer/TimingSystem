using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using TrailMeisterDb;

namespace TrailMeisterUtilities.Converters
{
    public enum TimeConversionType
    {
        BestLap,
        AverageLap,
        TotalTime,
    }
    public class Laps2TimeConverter :  IMultiValueConverter
    {

        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ulong timeMs = 0;
            TimeConversionPrecision precision = TimeConversionPrecision.ToTheHundredth;

            if (values.Length >= 2 &&
                    values[0] is TimeConversionType conversionType &&
                    values[1] is List<DbLap> eventLaps)
            {
                if (values.Length > 2 && values[2] != null)
                {
                    precision = (TimeConversionPrecision)values[2];
                }

                List<DbLap> laps = eventLaps.Where(l => l.LapTime > 0).ToList();

                if (laps.Count > 0) { 
                    switch (conversionType)
                    {
                        case TimeConversionType.BestLap:
                            timeMs = laps.Min(r => r.LapTime);
                            break;
                        case TimeConversionType.AverageLap:
                            timeMs = laps.Any()
                                ? (ulong)(laps.Aggregate<DbLap, decimal>(0, (sum, lap) => sum + lap.LapTime) / laps.Count)
                                : 0UL;
                            break;
                        case TimeConversionType.TotalTime:
                            timeMs = laps.Max(r => r.TotalTime);
                            break;
                    }
                }
            }

            Ms2TimeConverter c = new Ms2TimeConverter();
            return c.Convert(new object[] { timeMs, precision }, targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
}
}
