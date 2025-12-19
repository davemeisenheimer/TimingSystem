using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrailMeisterDb;
using TrailMeisterViewer.Model;
using TrailMeisterViewer.Windows.PersonalLog;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public partial class EventViewer : Window
    {
        public EventViewer()
        {
            InitializeComponent();
        }

        private void OnRacerRowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            RacerData racerData = (RacerData)(row.Item);

            PersonalLogController logController = new PersonalLogController(racerData.Person);
            logController.ShowWindow();
        }
    }
}
