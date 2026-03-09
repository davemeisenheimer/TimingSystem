using System.Windows.Controls;
using System.Windows.Input;

using TrailMeisterViewer.Windows.EventViewer;
using TrailMeisterDb;
using System.Windows;
using TrailMeisterUtilities;

namespace TrailMeisterViewer.Windows.MainWindow
{
    public partial class EventsTab : UserControl
    {
        public EventsTab()
        {
            InitializeComponent();
        }

        public RelayCommand RefreshCommand
        {
            get => (RelayCommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
        public static readonly DependencyProperty RefreshCommandProperty =
            DependencyProperty.Register("RefreshCommand", typeof(RelayCommand), typeof(EventsTab));

        private void OnEventRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var row = (DataGridRow)sender;
            DbEvent dbEvent = (DbEvent)(row.Item);

            var controller = new EventViewerController(dbEvent);
            var view = controller.CreateControl();
            (Application.Current.MainWindow as TrailMeisterViewer.MainWindow)?.Navigate(view, () => RefreshCommand.Execute(dbEvent));
        }

    }
}
