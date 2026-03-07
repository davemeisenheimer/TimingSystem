using TrailMeisterDb;
using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public class RacerEventRow
    {
        public DbEvent Event { get; set; }
        public RacerData RacerData { get; set; }
    }
}
