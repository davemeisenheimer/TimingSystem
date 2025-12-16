using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrailMeisterDb;
using TrailMeisterUtilities.Converters;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public class RacerData
    {
        private readonly Laps2TimeConverter laps2TimeConverter = new Laps2TimeConverter();
        internal RacerData(DbPerson person, List<DbLap> eventLaps)
        {
            this.Person = person;
            this.EventLaps = eventLaps;
            this.PersonId = person.PersonId;
        }

        public List<DbLap> EventLaps { get; set; }

        public DbPerson Person { get; set; }

        public int? PersonId { get; set; }

        public string BestLap
        {
            get
            {
                return (String)laps2TimeConverter.Convert(new object[] { TimeConversionType.BestLap, EventLaps }, typeof(object), null, CultureInfo.CurrentCulture);
            }
        }

        public string TotalTime
        {
            get
            {
                return (String)laps2TimeConverter.Convert(new object[] { TimeConversionType.TotalTime, EventLaps }, typeof(object), null, CultureInfo.CurrentCulture);
            }
        }

        public string AverageLap
        {
            get
            {
                return (String)laps2TimeConverter.Convert(new object[] { TimeConversionType.AverageLap, EventLaps }, typeof(object), null, CultureInfo.CurrentCulture);
            }
        }
    }
}
