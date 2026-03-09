using System.Windows;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public partial class ExportOptionsDialog : Window
    {
        public ExportOptionsDialog(bool hasHandicapData)
        {
            InitializeComponent();

            if (!hasHandicapData)
            {
                chkHandicap.IsEnabled = false;
                chkHandicap.ToolTip = "Toggle handicap mode on in the event viewer first to enable this option.";
            }
        }

        public ExportOptions? Result { get; private set; }

        private void OnPruneEarlyChecked(object sender, RoutedEventArgs e)
        {
            chkPruneLast.IsChecked = false;
            chkPruneLast.IsEnabled = false;
        }

        private void OnPruneEarlyUnchecked(object sender, RoutedEventArgs e)
        {
            chkPruneLast.IsEnabled = true;
        }

        private void OnPruneLastChecked(object sender, RoutedEventArgs e)
        {
            chkPruneEarly.IsChecked = false;
            chkPruneEarly.IsEnabled = false;
        }

        private void OnPruneLastUnchecked(object sender, RoutedEventArgs e)
        {
            chkPruneEarly.IsEnabled = true;
        }

        private void OnExport(object sender, RoutedEventArgs e)
        {
            if (chkRaw.IsChecked != true && chkHandicap.IsChecked != true)
            {
                MessageBox.Show("Please select at least one result type to export.",
                    "Export Options", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sortBy = rdoSortBestLap.IsChecked  == true ? ResultSortField.BestLap
                       : rdoSortAvgLap.IsChecked   == true ? ResultSortField.AverageLap
                       : rdoSortTotalTime.IsChecked == true ? ResultSortField.TotalTime
                       : ResultSortField.TotalLaps;

            Result = new ExportOptions
            {
                IncludeRawResults = chkRaw.IsChecked == true,
                IncludeHandicapResults = chkHandicap.IsChecked == true,
                PruneEarlyLaps = chkPruneEarly.IsChecked == true,
                PruneLastLaps = chkPruneLast.IsChecked == true,
                SortBy = sortBy,
                IncludeRanking = chkRanking.IsChecked == true,
            };
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
