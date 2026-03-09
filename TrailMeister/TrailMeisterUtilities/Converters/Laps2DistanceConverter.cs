using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using TrailMeisterDb;

namespace TrailMeisterUtilities.Converters
{
    public class Laps2DistanceConverter : IMultiValueConverter
    {

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double distanceMetres = 0;
            string formattedDistance = "0m";

            if (values.Length >= 1 &&
                    values[0] is List<DbLap> laps)
            {
                var eventIds = laps
                        .Select(l => l.EventId)
                        .Distinct()
                        .ToList();

                DbEventsTable eventsTable = new DbEventsTable();
                List<DbEvent> dbEvents = eventsTable.getEvents();

                var lapLengthsByEventId = dbEvents
                        .Where(e => eventIds.Contains(e.ID))
                        .ToDictionary(
                            e => e.ID,
                            e => e.LapLength
                        );

                foreach (var lap in laps.Where(l => l.LapCount > 0))
                {
                    int eventDefault = lapLengthsByEventId.TryGetValue((uint)lap.EventId, out int def) ? def : 0;
                    distanceMetres += lap.LapLength ?? eventDefault;
                }

                formattedDistance = distanceMetres.ToString() + "m";
            }

            if (distanceMetres > 1000)
            {
                double distanceKm = distanceMetres / 1000;
                formattedDistance = String.Format("{0:0.##}km", distanceKm);
            }

            return formattedDistance;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
