using System.Windows.Controls;
using System.Windows.Input;

using TrailMeisterViewer.Windows.EventViewer;
using TrailMeisterDb;
using System.ComponentModel;
using System.Windows;
using TrailMeisterUtilities;

namespace TrailMeisterViewer.Windows.MainWindow
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EventsTab : UserControl
    {
        public EventsTab()
        {
            InitializeComponent();
            SetInitialSortOrder();
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

            EventViewerController eventViewerController = new EventViewerController(dbEvent);
            eventViewerController.ShowWindow();

            // FixMe: This is ugly and likely a bad pattern.
            this.RefreshCommand.Execute(dbEvent);
        }
        private void SetInitialSortOrder()
        {
            gridEvents.Items.SortDescriptions.Clear();
            gridEvents.Items.SortDescriptions.Add(new SortDescription(nameof(DbEvent.EventDate), ListSortDirection.Descending));
        }
    }
}
