using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TrailMeister.Model;
using TrailMeister.Model.Data;
using TrailMeisterUtilities;
using TrailMeisterDb;

namespace TrailMeister.GUI.Main
{
    internal class MainWindowVM : Disposable, INotifyPropertyChanged
    {
        private Timers? _recentArrivalPageTimeout;
        protected Main _mainWindow;
        private ReaderStatus _readerStatus;
        private ObservableKeyedCollection<int, DbTag> _allTags = new ObservableKeyedCollection<int, DbTag>(null, "TagId");
        private ObservableKeyedCollection<int, RecentLapData> _recentLapData = new ObservableKeyedCollection<int, RecentLapData>(null, "ID");
        private RecentLapData? _mostRecentData;
        private ObservableKeyedCollection<string, RecentLapData> _allParticipants = new ObservableKeyedCollection<string, RecentLapData>(null, "EPC");
        private ObservableKeyedCollection<int, DbPerson> _allPeople = new ObservableKeyedCollection<int, DbPerson>(null, "PersonId");
        protected readonly MainWindowController _controller;

        private string _eventName = "Event " + DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM/dd");
        private int _antennaPower = 5;
        bool _eventStarted = false;
        bool _isEventFinished = false;

        internal MainWindowVM(Main mainWindow)
        {
            _mainWindow = mainWindow;
            this._controller = initController();
            ButtonCommand_StartEvent = new ButtonCommand(o => this._controller.StartEvent());
            ButtonCommand_ConnectReader = new ButtonCommand(o => this._controller.ConnectReader());
            EnableArduinoConfig = true;
        }

        protected virtual MainWindowController initController()
        {
            return new MainWindowController(this);
        }
        public ICommand ButtonCommand_StartEvent { get; set; }
        public ICommand ButtonCommand_ConnectReader { get; set; }

        public bool EventStarted
        {
            get => _eventStarted;
            set
            {
                if (value != _eventStarted)
                {
                    _eventStarted = value;
                    OnPropertyChanged(nameof(EventStarted));
                }
            }
        }

        public bool IsEventFinished
        {
            get => _isEventFinished;
            set
            {
                if (value != _isEventFinished)
                {
                    _isEventFinished = value;
                    OnPropertyChanged(nameof(IsEventFinished));
                }
            }
        }
        public ReaderStatus ReaderStatus
        {
            get
            {
                return _readerStatus;
            }
            set
            {
                if (value != _readerStatus)
                {
                    _readerStatus = value;
                    OnPropertyChanged(nameof(ReaderStatus));
                }
            }
        }
        public string EventName
        {
            get
            {
                return this._eventName;
            }
            set
            {
                if (this._eventName != value)
                {
                    this._eventName = value;
                    OnPropertyChanged(nameof(EventName));
                }
            }
        }

        public int AntennaPower
        {
            get
            {
                return this._antennaPower;
            }
            set
            {
                if (this._antennaPower != value)
                {
                    this._antennaPower = value;
                    OnPropertyChanged(nameof(AntennaPower));
                    OnPropertyChanged(nameof(AntennaPowerLabel));
                }
            }
        }

        public string AntennaPowerLabel
        {
            get
            {
                return string.Format("Antenna power: {0}dBm", this._antennaPower);
            }
        }

        public ObservableKeyedCollection<int, RecentLapData> RecentLapData
        {
            get
            {
                return _recentLapData;
            }
            set
            {
                if (_recentLapData != value)
                {
                    _recentLapData = value;
                    OnPropertyChanged(nameof(RecentLapData));
                }
            }
        }

        public RecentLapData? MostRecent
        {
            get
            {
                return _mostRecentData;
            }
            set
            {
                if (value != _mostRecentData)
                {
                    _mostRecentData = value;
                    OnPropertyChanged(nameof(MostRecent));
                }
            }
        }

        public ObservableKeyedCollection<string, RecentLapData> AllParticipants
        {
            get
            {
                return _allParticipants;
            }
            set
            {
                if (_allParticipants != value)
                {
                    _allParticipants = value;
                    OnPropertyChanged(nameof(AllParticipants));
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

        public bool EnableArduinoConfig { get; set; }

        internal void UpdateRecentData(RecentLapData lapData)
        {

            //Task.Factory.StartNew(() =>
            //{
                _mainWindow.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        RecentLapData? existingTag;

                        if (!this._recentLapData.TryGetValue(lapData.ID, out existingTag))
                        {
                            addTag(lapData);
                        } else
                        {
                            this._recentLapData.Remove(lapData.ID);
                            this._recentLapData.Insert(0, lapData);
                        }
                        MostRecent = lapData;
                        GotoArrivalsPage();
                    })
                );
            //});
        }

        private void GotoArrivalsPage()
        {
            DisposeTimer(_recentArrivalPageTimeout);
            _recentArrivalPageTimeout = Timers.Delay(
                                            12000,
                                            new Action(() =>
                                                _mainWindow.Dispatcher.BeginInvoke(
                                                    new Action(() =>
                                                    {
                                                        this._mostRecentData = null;
                                                        this._recentLapData.Clear();
                                                        GotoAllDataPage();
                                                    })
                                                )));
            if (RecentLapData.Count == 1)
            {
                GotoOneArrivalPage();
            }
            else
            {
                GotoRecentArrivalsPage();
            }
        }

        internal void GotoNewEventPage()
        {
            _mainWindow.gotoNewEventPage();
        }

        internal void GotoRecentArrivalsPage()
        {
            if (EnableArduinoConfig)
            {
                // Only give one chance to set up the arduino, then stop its listener to prevent
                // its system resources being consumed by eternally checking for incoming messages
                // EnableArduinoConfig = false; 
                // _controller.stopArduinoListener();
            }
            _mainWindow.gotoRecentArrivalsPage();
        }

        internal void GotoOneArrivalPage()
        {
            _mainWindow.gotoOneArrivalPage();
        }

        internal void GotoAllDataPage()
        {
            _mainWindow.gotoAllDataPage();
            DisposeTimer(_recentArrivalPageTimeout);
        }

        private void DisposeTimer(Timers? timer)
        {
            if (timer != null && !timer.IsDisposed)
            {
                timer.Dispose();
                timer = null;
            }
        }

        // Private Methods
        private void addTag(RecentLapData tagData)
        {
            // Allow 10 recent arrivals at a time
            if (this._recentLapData.Count > 10)
            {
                this.RecentLapData.Remove(this.RecentLapData.Last().ID);
            }
            this.RecentLapData.Insert(0, tagData);
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
                    DisposeTimer(_recentArrivalPageTimeout);
                }
            }

            //dispose unmanaged resources
            this._controller.Dispose();
            _disposed = true;
        }
    }
}
