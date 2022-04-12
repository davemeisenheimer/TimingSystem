using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TrailMeister.Model;
using TrailMeister.Model.Data;
using TrailMeister.Model.Helpers;
using TrailMeisterDb;

namespace TrailMeister.GUI.Main
{
    internal class MainWindowVM : Disposable, INotifyPropertyChanged
    {
        private Timers? _recentArrivalPageTimeout;
        private Main _mainWindow;
        private ReaderStatus _readerStatus;
        private ObservableKeyedCollection<int, DbTag> _allTags = new ObservableKeyedCollection<int, DbTag>(null, "TagId");
        private ObservableKeyedCollection<int, RecentLapData> _recentData = new ObservableKeyedCollection<int, RecentLapData>(null, "ID");
        private RecentLapData? _mostRecentData;
        private ObservableKeyedCollection<string, RecentLapData> _allParticipants = new ObservableKeyedCollection<string, RecentLapData>(null, "EPC");
        private readonly MainWindowController _controller;

        private string _eventName = "Event " + DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM/dd");
        private int _antennaPower = 5;
        bool _eventStarted = false;

        internal MainWindowVM(Main mainWindow)
        {
            _mainWindow = mainWindow;
            this._controller = new MainWindowController(this);
            ButtonCommand_StartEvent = new ButtonCommand(o => this._controller.StartEvent());
            ButtonCommand_ConnectReader = new ButtonCommand(o => this._controller.ConnectReader());
            EnableArduinoConfig = true;

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

        public ObservableKeyedCollection<int, RecentLapData> RecentData
        {
            get
            {
                return _recentData;
            }
            set
            {
                if (_recentData != value)
                {
                    _recentData = value;
                    OnPropertyChanged(nameof(RecentData));
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

                        if (!this._recentData.TryGetValue(lapData.ID, out existingTag))
                        {
                            addTag(lapData);
                        } else
                        {
                            this._recentData.Remove(lapData.ID);
                            this._recentData.Insert(0, lapData);
                        }
                        MostRecent = lapData;
                        GotoArrivalsPage();
                    })
                );
            //});
        }

        private void GotoArrivalsPage()
        {
            _recentArrivalPageTimeout = Timers.Delay(
                                            7000,
                                            new Action(() =>
                                                _mainWindow.Dispatcher.BeginInvoke(
                                                    new Action(() => GotoAllDataPage())
                                                    )));
            if (RecentData.Count == 1)
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
            this._controller.StartEvent();
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
            if (this._recentData.Count > 10)
            {
                this.RecentData.Remove(this.RecentData.Last().ID);
            }
            this.RecentData.Insert(0, tagData);
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
