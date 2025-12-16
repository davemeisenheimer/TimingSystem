using System.ComponentModel;
using System.Windows.Input;
using TrailMeisterUtilities;
using TrailMeisterDb;

namespace TrailMeisterViewer
{
    internal class MainWindowVM : Disposable, INotifyPropertyChanged
    {
        protected MainWindow _mainWindow;
        protected readonly MainWindowController _controller;

        private ObservableKeyedCollection<int, DbTag> _allTags = new ObservableKeyedCollection<int, DbTag>(null, "TagId");
        private ObservableKeyedCollection<int, DbEvent> _allEvents = new ObservableKeyedCollection<int, DbEvent>(null, "ID");
        private ObservableKeyedCollection<int, DbPerson> _allPeople = new ObservableKeyedCollection<int, DbPerson>(null, "PersonId");

        internal MainWindowVM(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            this._controller = initController();
            ButtonCommand_AddPerson = new ButtonCommand(o => this._controller.AddNewPerson());

            DeleteEventCommand = new ButtonCommand(
                                        o => this._controller.DeleteEventClicked(o), 
                                        o => this._controller.CanExecuteDeleteEvent(o));
        }

        protected virtual MainWindowController initController()
        {
            return new MainWindowController(this);
        }
        public ICommand ButtonCommand_AddPerson { get; set; }
        public ICommand DeleteEventCommand { get; set; }

        public ObservableKeyedCollection<int, DbTag> AllTags
        {
            get
            {
                return _allTags;
            }
            set
            {
                if (_allTags != value)
                {
                    _allTags = value;
                    OnPropertyChanged(nameof(AllTags));
                }
            }
        }

        public ObservableKeyedCollection<int, DbEvent> AllEvents
        {
            get
            {
                return _allEvents;
            }
            set
            {
                if (_allEvents != value)
                {
                    _allEvents = value;
                    OnPropertyChanged(nameof(AllEvents));
                }
            }
        }

        public ObservableKeyedCollection<int, DbPerson> AllPeople
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

        // Overrides

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
            }

            //dispose unmanaged resources
            this._controller.Dispose();
            _disposed = true;
        }
    }
}
