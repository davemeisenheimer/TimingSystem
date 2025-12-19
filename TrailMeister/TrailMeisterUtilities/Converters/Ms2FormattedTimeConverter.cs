using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TrailMeisterDb;

namespace TrailMeisterUtilities.Converters
{
    public enum TimeConversionPrecision
    {
        ToTheMinute,
        ToTheSecond,
        ToTheTenth,
        ToTheHundredth,
        ToTheMillisecond,
    }

    public class Ms2TimeConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ulong timeMs = (ulong)values[0];
            TimeConversionPrecision precision = TimeConversionPrecision.ToTheHundredth;

            if (values.Length > 1 && values[1] != null)
            {
                precision = (TimeConversionPrecision)values[1];
            }

            // Convert milliseconds to TimeSpan
            TimeSpan t = TimeSpan.FromMilliseconds(timeMs);

            string formatted;

            switch (precision)
            {
                case TimeConversionPrecision.ToTheMinute:
                    formatted = t.TotalHours >= 1
                        ? string.Format("{0}:{1:D2}", (int)t.TotalHours, t.Minutes)
                        : string.Format("{0:D2}", t.Minutes);
                    break;
                case TimeConversionPrecision.ToTheSecond:
                    formatted = t.TotalHours >= 1
                        ? string.Format("{0}:{1:D2}:{2:D2}", (int)t.TotalHours, t.Minutes, t.Seconds)
                        : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
                    break;
                case TimeConversionPrecision.ToTheTenth:
                    formatted = t.TotalHours >= 1
                        ? string.Format("{0}:{1:D2}:{2:D2}.{3:D1}", (int)t.TotalHours, t.Minutes, t.Seconds, t.Milliseconds)
                        : string.Format("{0:D2}:{1:D2}.{2:D1}", t.Minutes, t.Seconds, t.Milliseconds);
                    break;
                case TimeConversionPrecision.ToTheMillisecond:
                    formatted = t.TotalHours >= 1
                        ? string.Format("{0}:{1:D2}:{2:D2}.{3:D3}", (int)t.TotalHours, t.Minutes, t.Seconds, t.Milliseconds)
                        : string.Format("{0:D2}:{1:D2}.{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);
                    break;
                case TimeConversionPrecision.ToTheHundredth:
                default:
                    formatted = t.TotalHours >= 1
                        ? string.Format("{0}:{1:D2}:{2:D2}.{3:D2}", (int)t.TotalHours, t.Minutes, t.Seconds, t.Milliseconds)
                        : string.Format("{0:D2}:{1:D2}.{2:D2}", t.Minutes, t.Seconds, t.Milliseconds);
                    break;
            }

            return formatted;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
