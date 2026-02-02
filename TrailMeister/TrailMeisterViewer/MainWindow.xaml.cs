using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TrailMeisterViewer.Windows.EventViewer;
using TrailMeisterDb;

namespace TrailMeisterViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM _viewModel;
        public MainWindow()
        {
            this._viewModel = new MainWindowVM(this);
            this.DataContext = this._viewModel;
            InitializeComponent();
        }

        private void OnEventRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            DbEvent dbEvent = (DbEvent)(row.Item);

            EventViewerController eventViewerController = new EventViewerController(dbEvent);
            eventViewerController.ShowWindow();

            // FixMe: This is ugly and likely a bad pattern.
            this._viewModel.RelayCommand_Refresh.Execute(dbEvent);
        }
    }
}
