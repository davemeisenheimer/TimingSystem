using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrailMeisterDb;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public partial class PersonalLog : Window
    {
        public PersonalLog()
        {
            InitializeComponent();
        }

        private void OnRacerRowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            DbEvent dbEvent = (DbEvent)(row.Item);
            // Currently a noop here, but could do something, if we want to
            // Originally planned to open another window with laps summary for this racer, but 
            // we're using a tooltip instead, for the time being.
        }
    }
}
