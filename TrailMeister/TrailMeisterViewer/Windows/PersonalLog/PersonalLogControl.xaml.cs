using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrailMeisterViewer.Windows.PersonalLog
{
    public partial class PersonalLogControl : UserControl
    {
        public PersonalLogControl()
        {
            InitializeComponent();
        }

        private void OnEventRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow row || row.Item is not RacerEventRow racerEventRow)
                return;

            e.Handled = true;

            var view = new LapDetailControl { DataContext = racerEventRow };
            (Application.Current.MainWindow as TrailMeisterViewer.MainWindow)?.Navigate(view);
        }
    }
}
