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
    public class Ms2TimeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ulong timeMs = (ulong)value;

            // Convert milliseconds to TimeSpan
            TimeSpan t = TimeSpan.FromMilliseconds(timeMs);

            // Format as H:MM:SS.mmm or M:SS.mmm depending on total hours
            string formatted = t.TotalHours >= 1
                ? string.Format("{0}:{1:D2}:{2:D2}.{3:D3}", (int)t.TotalHours, t.Minutes, t.Seconds, t.Milliseconds)
                : string.Format("{0}:{1:D2}.{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);

            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
