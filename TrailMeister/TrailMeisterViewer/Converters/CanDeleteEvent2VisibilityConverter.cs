using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using TrailMeisterDb;

namespace TrailMeisterViewer.Converters
{
    public class CanDeleteEventToVisibilityConverter : IValueConverter
    {
        private DbLapsTable _dbLapsTable = new DbLapsTable();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dbEvent = value as DbEvent;
            if (dbEvent == null)
                return Visibility.Collapsed;

            // Same logic as CanExecute
            return !_dbLapsTable.getEventLapsForEvent(dbEvent.ID).Any()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
