using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrailMeisterViewer.Model;
using TrailMeisterViewer.Windows.PersonalLog;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public partial class EventViewerControl : UserControl
    {
        public EventViewerControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private EventViewerVM? _vm;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm != null)
                _vm.PropertyChanged -= OnVmPropertyChanged;

            _vm = DataContext as EventViewerVM;

            if (_vm != null)
                _vm.PropertyChanged += OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EventViewerVM.IsHandicapMode))
                ApplyHandicapSort(_vm!.IsHandicapMode);
        }

        private void ApplyHandicapSort(bool isHandicapMode)
        {
            gridParticipants.Items.SortDescriptions.Clear();
            if (isHandicapMode)
            {
                gridParticipants.Items.SortDescriptions.Add(
                    new SortDescription(nameof(RacerData.AdjustedBestLapMs), ListSortDirection.Ascending));
            }
        }

        private void OnRacerRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow row || row.IsEditing || row.Item is not RacerData racerData)
                return;

            e.Handled = true;

            var logController = new PersonalLogController(racerData.Person);
            var view = logController.CreateControl();
            (Application.Current.MainWindow as TrailMeisterViewer.MainWindow)?.Navigate(view);
        }
    }
}
