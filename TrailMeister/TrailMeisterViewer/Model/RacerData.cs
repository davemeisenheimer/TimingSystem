using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrailMeisterDb;
using TrailMeisterUtilities.Converters;

namespace TrailMeisterViewer.Model
{
    public class RacerData
    {
        private readonly Laps2TimeConverter laps2TimeConverter = new Laps2TimeConverter();
        internal RacerData(DbPerson person, List<DbLap> eventLaps)
        {
            this.Person = person;
            this.Laps = eventLaps;
            this.PersonId = person.PersonId;
        }

        // Laps is a collection that can store laps for multiple contexts:
        //  e.g. could be for a given event, a given season, or all time
        public List<DbLap> Laps { get; set; }

        public DbPerson Person { get; set; }

        public long PersonId { get; set; }

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
