using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

            double savedScrollOffset = FindScrollViewer(gridParticipants)?.VerticalOffset ?? 0;

            var view = new LapDetailControl { DataContext = racerEventRow };
            (Application.Current.MainWindow as TrailMeisterViewer.MainWindow)?.Navigate(view, () =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    FindScrollViewer(gridParticipants)?.ScrollToVerticalOffset(savedScrollOffset));
            });
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject element)
        {
            if (element is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var result = FindScrollViewer(VisualTreeHelper.GetChild(element, i));
                if (result != null) return result;
            }
            return null;
        }
    }
}
