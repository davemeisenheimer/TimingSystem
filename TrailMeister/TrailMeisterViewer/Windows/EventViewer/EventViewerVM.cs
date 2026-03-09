
using TrailMeisterUtilities;
using TrailMeisterDb;
using TrailMeisterViewer.Model;

namespace TrailMeisterViewer.Windows.EventViewer
{
    public class EventViewerVM : ViewModelBase
    {
        private ObservableKeyedCollection<long, RacerData> _allRacerData = new ObservableKeyedCollection<long, RacerData>(null, "PersonId");
        private ObservableKeyedCollection<long, DbPerson> _allPeople = new ObservableKeyedCollection<long, DbPerson>(null, "PersonId");
        private EventViewerController controller;

        public EventViewerVM(EventViewerController c, DbEvent dbEvent)
        {
            Event = dbEvent;
            controller = c;
            ExportHtmlCommand = new ButtonCommand(ExecuteExportHtml, CanExecuteExportHtml);
            SelectedPersonChangedCommand = new RelayCommand(o => controller.ExecuteOnPersonChanged((RacerData)o), CanExecuteOnPersonChanged);
            ToggleHandicapCommand = new ButtonCommand(ExecuteToggleHandicap, _ => true);
        }

        private bool CanExecuteExportHtml(object? obj) => true;

        private void ExecuteExportHtml(object obj) => controller.ExportHtml();

        private void ExecuteToggleHandicap(object obj)
        {
            // IsHandicapMode is already updated by the TwoWay IsChecked binding before this fires
            if (IsHandicapMode)
                controller.LoadHandicaps();
        }

        private bool CanExecuteOnPersonChanged(object? obj)
        {
            return true;
        }

        private bool _isHandicapMode;
        public bool IsHandicapMode
        {
            get => _isHandicapMode;
            set
            {
                if (_isHandicapMode != value)
                {
                    _isHandicapMode = value;
                    OnPropertyChanged(nameof(IsHandicapMode));
                }
            }
        }

        public DbEvent Event { get; private set; }

        public ObservableKeyedCollection<long, RacerData> AllRacerData
        {
            get
            {
                return _allRacerData;
            }
            set
            {
                if (_allRacerData != value)
                {
                    _allRacerData = value;
                    OnPropertyChanged(nameof(AllRacerData));
                }
            }
        }

        public ObservableKeyedCollection<long, DbPerson> AllPeople
        {
            get
            {
                return _allPeople;
            }
            set
            {
                if (_allPeople != value)
                {
                    _allPeople = value;
                    OnPropertyChanged(nameof(AllPeople));
                }
            }
        }

        public ButtonCommand ExportHtmlCommand { get; set; }
        public ButtonCommand ToggleHandicapCommand { get; set; }
        public RelayCommand SelectedPersonChangedCommand { get; set; }
    }
}
