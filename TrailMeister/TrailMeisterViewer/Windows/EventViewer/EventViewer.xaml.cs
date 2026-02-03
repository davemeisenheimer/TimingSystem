using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private void OnRacerRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow row || row.IsEditing || row.Item is not RacerData racerData)
                return;

            // Stop bubbling
            e.Handled = true;

            // Open the PersonalLog window
            var logController = new PersonalLogController(racerData.Person);
            logController.ShowWindow();
        }

    }
}
