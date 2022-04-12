using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeister.Model;
using TrailMeisterDb;
using TrailMeister.Model.Data;
using System.Diagnostics;
using TrailMeister.Model.Helpers;

namespace TrailMeister.GUI.Main
{
    internal class MainWindowController : Disposable
    {
        TaskFactory _uiFactory;
        private object _locker = new object();
        long _eventId;
        MainWindowVM _vm;
        ITagDataSource _tagReader = new TagReader(TagReaderDataSourceType.M6ENano);
        DbTagsTable _dbTagsTable = new DbTagsTable();
        DbLapsTable _dbLapsTable = new DbLapsTable();
        DbEventsTable _dbEventsTable = new DbEventsTable();

        public MainWindowController(MainWindowVM vm)
        {
            _uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            this._vm = vm;
            // Save default event name
            this._eventId = _dbEventsTable.addEvent(this._vm.EventName);

            _tagReader.TagDataSourceEvent += this.OnTagDataSourceEvent;
            vm.ReaderStatus = ReaderStatus.Disconnected;
            _tagReader.init();
        }

        public void ConnectReader()
        {
            _tagReader.init();
        }

        public void StartEvent()
        {
            SetAntennaPower();
            _dbEventsTable.updateEvent(this._eventId, this._vm.EventName);
            _vm.EventStarted = true;
        }

        internal void SetAntennaPower()
        {
            _tagReader.Config.SetAntennaPower(_vm.AntennaPower * 100);
        }

        private RecentLapData? AddNewReaderData(ReaderData data)
        {
            DbTag? tag = _dbTagsTable.getTag(data.EPC);

            if (tag != null)
            {
                AddTag(tag);

                if (_vm.EventStarted)
                {
                    return AddLapData(data, tag);
                }
            }
            return null;
        }

        private void AddTag(DbTag tag)
        {
            if (!_vm.AllTags.TryGetValue(tag.TagId, out _))
            {
                this._vm.AllTags.Add(tag);
            }
        }

        private RecentLapData AddLapData(ReaderData lapData, DbTag tag)
        {
            RecentLapData recentLapData = new RecentLapData(tag.TagId, tag.HumanName, tag.EPC, lapData.TimeStamp);
            recentLapData.PropertyChanged += this.OnRecentLapDataChanged;

            lock (this._locker)
            {
                this._vm.AllParticipants.Add(recentLapData);
            }
            return recentLapData;
        }

        private void OnRecentLapDataChanged(object? sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "Name") {
                RecentLapData? lapData = sender as RecentLapData;

                if (lapData != null)
                {
                    _dbTagsTable.updateTag(lapData.ID, lapData.Name);
                }
            }
        }

        private void OnTagDataSourceEvent(object sender, TagDataEventArgs args)
        {
            if (args.Type == TagDataSourceEventType.LapData)
            {
                _uiFactory.StartNew(() => HandleNewLapEvent(args));
            }

            _vm.ReaderStatus = args.ReaderStatus;
        }


        private void HandleNewLapEvent(TagDataEventArgs args)
        {
            switch (args.Type)
            {
                case TagDataSourceEventType.LapData:
                    UpdateTagData(args.RecentLapData);
                    break;

                case TagDataSourceEventType.Connected:
                    break;

                case TagDataSourceEventType.Disconnected:
                    break;
                default:
                    break;

            }
        }

        private void UpdateTagData(ReaderData? lapData)
        {
            if (lapData == null)
            {
                Debug.WriteLine("Error: Got a LapData event without the lapData");
                return;
            }

            RecentLapData? tagToUpdate = null;
            bool haveTagAlready = false;

            lock (this._locker)
            {
                haveTagAlready = this._vm.AllParticipants.TryGetValue(lapData.EPC, out tagToUpdate);
            }

            if (!haveTagAlready)
            {
                // First time we've seen this tag at this event (i.e. they've just crossed the start line)
                tagToUpdate = this.AddNewReaderData(lapData);
            } else if (tagToUpdate != null)
            {
                tagToUpdate.update(lapData.TimeStamp);
            }

            if (tagToUpdate != null)
            {
                // Give the ViewModel a chance to update its list of recent updates
                this._vm.UpdateRecentData(tagToUpdate);
            }

            if (tagToUpdate != null)
            {
                _dbLapsTable.addLap(tagToUpdate.ID, _eventId, tagToUpdate.LapCount, tagToUpdate.TimeLapMs, tagToUpdate.TimeTotalMs, tagToUpdate.Name);
            }
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
            _tagReader.TagDataSourceEvent -= this.OnTagDataSourceEvent;
            _tagReader.Dispose();

            _disposed = true;
        }
    }
}
