using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TrailMeisterViewer.Windows.EventViewer;
using TrailMeisterDb;
using System.ComponentModel;
using System;

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
            SetInitialSortOrder();
        }

        private void OnEventRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var row = (DataGridRow)sender;
            DbEvent dbEvent = (DbEvent)(row.Item);

            EventViewerController eventViewerController = new EventViewerController(dbEvent);
            eventViewerController.ShowWindow();

            // FixMe: This is ugly and likely a bad pattern.
            this._viewModel.RelayCommand_Refresh.Execute(dbEvent);
        }
        private void SetInitialSortOrder()
        {
            gridEvents.Items.SortDescriptions.Clear();
            gridEvents.Items.SortDescriptions.Add(new SortDescription(nameof(DbEvent.EventDate), ListSortDirection.Descending));
        }

    }
}
