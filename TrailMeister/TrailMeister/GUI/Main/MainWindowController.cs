using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using TrailMeister.Model;
using TrailMeisterDb;
using TrailMeister.Model.Data;
using System.Diagnostics;
using TrailMeisterUtilities;

namespace TrailMeister.GUI.Main
{
    internal class MainWindowController : Disposable
    {
        TaskFactory _uiFactory;
        private object _locker = new object();
        long _eventId;
        ITagDataSource _tagReader = new TagReader(TagReaderDataSourceType.M6ENano);
        DbPeopleTable _dbPeopleTable = new DbPeopleTable();
        DbTagsTable _dbTagsTable = new DbTagsTable();
        DbLapsTable _dbLapsTable = new DbLapsTable();
        DbEventsTable _dbEventsTable = new DbEventsTable();

        protected MainWindowVM _vm;

        public MainWindowController(MainWindowVM vm)
        {
            _uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            this._vm = vm;
            init();
        }

        protected virtual void init()
        {
            //this._vm.PropertyChanged += OnViewModelPropertyChanged;

            // Save default event name
            this._eventId = _dbEventsTable.addEvent(this._vm.EventName);

            _tagReader.TagDataSourceEvent += this.OnTagDataSourceEvent;
            this._vm.ReaderStatus = ReaderStatus.Disconnected;
            _tagReader.init();

            AddPeople();
        }

        private void AddPeople()
        {
            List<DbPerson> people = _dbPeopleTable.getPeople();

            foreach (DbPerson person in people)
            {
                this._vm.AllPeople.Add(person);
            }
        }

        private void OnViewModelPropertyChanged(string propertyName)
        {
            switch(propertyName)
            {
                case nameof(this._vm.IsEventFinished):
                    OnFinishEvent();
                    break;
                default:
                    break;
            }
        }

        /* Allows for prepopulating the event with tags, rather than having to pass each tag
         * in front of the antenna to add them to the event.
         */
        private void AddTagsToEvent()
        {
            Debug.WriteLine("MainWindowController: AddTagsToEvent");

            // ID 29 - Dave
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001D8", DateTime.Now, 888)));
            // ID 30 = Heather
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2019112911861A0110100315", DateTime.Now, 888)));
            //// ID 31
            //OnTagDataSourceEvent(
            //    this,
            //    new TagDataEventArgs(
            //        TagDataSourceEventType.LapData,
            //        "Tag read from M6ENano",
            //        new ReaderData("2019112911861A01101001D1", DateTime.Now, 888)));
            //// ID 32
            //OnTagDataSourceEvent(
            //    this,
            //    new TagDataEventArgs(
            //        TagDataSourceEventType.LapData,
            //        "Tag read from M6ENano",
            //        new ReaderData("2019112911861A01101001DF", DateTime.Now, 888)));

            // ID 33 (Adam)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2019112911861A01101001A3", DateTime.Now, 888)));
            //// ID 34
            //OnTagDataSourceEvent(
            //     this,
            //     new TagDataEventArgs(
            //         TagDataSourceEventType.LapData,
            //         "Tag read from M6ENano",
            //         new ReaderData("2019112911861A01101001A2", DateTime.Now, 888)));

            // ID 35 (Dan)
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001CF", DateTime.Now, 888)));
            // ID 36 (Elliot)
            OnTagDataSourceEvent(
                this,
                new TagDataEventArgs(
                    TagDataSourceEventType.LapData,
                    "Tag read from M6ENano",
                    new ReaderData("2019112911861A01101001D7", DateTime.Now, 888)));

            //// ID 37
            //OnTagDataSourceEvent(
            //     this,
            //     new TagDataEventArgs(
            //         TagDataSourceEventType.LapData,
            //         "Tag read from M6ENano",
            //         new ReaderData("2021090611861A0110100270", DateTime.Now, 888)));
            //// ID 38
            //OnTagDataSourceEvent(
            //     this,
            //     new TagDataEventArgs(
            //         TagDataSourceEventType.LapData,
            //         "Tag read from M6ENano",
            //         new ReaderData("2019112911861A01101001A4", DateTime.Now, 888)));
            //// ID 39
            //OnTagDataSourceEvent(
            //     this,
            //     new TagDataEventArgs(
            //         TagDataSourceEventType.LapData,
            //         "Tag read from M6ENano",
            //         new ReaderData("2019112911861A01101001CD", DateTime.Now, 888)));

            // ID 41 (Justin)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100759", DateTime.Now, 888)));

            // ID 42 (Colin)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100752", DateTime.Now, 888)));

            // ID 44 (Oden)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042220085A01101006A7", DateTime.Now, 888)));

            // ID 45 (Rylan)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042220085A0110100696", DateTime.Now, 888)));

            // ID 46 (Jack)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A011010075B", DateTime.Now, 888)));

            // ID 47 (Sam)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100753", DateTime.Now, 888)));

            // ID 48 (Elias)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100773", DateTime.Now, 888)));

            // ID 49 (Oskar)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A011010075A", DateTime.Now, 888)));

            // ID 50 (Dani)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042220085A0110100020", DateTime.Now, 888)));

            // ID 51 (Daria)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A011010074C", DateTime.Now, 888)));

            // ID 52 (Lili)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100774", DateTime.Now, 888)));

            // ID 53 (Zoe)
            OnTagDataSourceEvent(
                 this,
                 new TagDataEventArgs(
                     TagDataSourceEventType.LapData,
                     "Tag read from M6ENano",
                     new ReaderData("2025042120085A0110100754", DateTime.Now, 888)));

        }

        public void ConnectReader()
        {
            _tagReader.init();
        }

        public virtual void StartEvent()
        {
            SetAntennaPower();
            _dbEventsTable.updateEvent(this._eventId, this._vm.EventName);
            _vm.EventStarted = true;
        }

        public virtual void OnFinishEvent()
        {
            _dbEventsTable.updateEvent(this._eventId, this._vm.EventName, this._vm.IsEventFinished);
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
                tag.PropertyChanged += this.OnDbTagChanged;
                this._vm.AllTags.Add(tag);
            }
        }

        // Currently unused. Tag associations can be edited in the Viewer app. We
        // could change the tag association here to use the same combobox as in the 
        // viewer app though.  Then this function would be here to handle changes to 
        // the tag association with a DbPerson. Wouldn't be filtering on HumanName at
        // that point, as that property of a DbTag has been removed.
        private void OnDbTagChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "HumanName")
            {
                DbTag? tag = sender as DbTag;

                if (tag != null)
                {
                    _dbTagsTable.updateTag(tag.TagId, tag.PersonId);
                }
            }
        }

        private RecentLapData AddLapData(ReaderData lapData, DbTag tag)
        {
            RecentLapData recentLapData = new RecentLapData(tag.TagId, tag.PersonId, tag.EPC, lapData.TimeStamp);
            //recentLapData.PropertyChanged += this.OnRecentLapDataChanged;

            lock (this._locker)
            {
                this._vm.AllParticipants.Add(recentLapData);
            }
            return recentLapData;
        }

        protected void OnTagDataSourceEvent(object sender, TagDataEventArgs args)
        {
            if (args.Type == TagDataSourceEventType.LapData)
            {
                _uiFactory.StartNew(() => HandleNewLapEvent(args));
            }
            else if (args.Type == TagDataSourceEventType.Connected && _vm.ReaderStatus != ReaderStatus.Connected)
            {
                // Putting this here instead of the init function, so that the connection screens can do their thing.
                if (this._vm.AllTags.Count == 0)
                {
                    AddTagsToEvent();
                }
            }
            else if (args.Type == TagDataSourceEventType.Disconnected)
            {
                _vm.AllTags.Clear();
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
                DbTag? dbTag = null;
                int? personId = tagToUpdate.PersonId;
                lock (this._locker)
                {
                    if (_vm.AllTags.TryGetValue(tagToUpdate.ID, out dbTag))
                    {
                        personId = dbTag.PersonId;
                    }
                }

                tagToUpdate.update(lapData.TimeStamp, personId);
            }

            if (tagToUpdate != null)
            {
                // Give the ViewModel a chance to update its list of recent updates
                this._vm.UpdateRecentData(tagToUpdate);
            }

            if (tagToUpdate != null)
            {
                _dbLapsTable.addLap(tagToUpdate.ID, _eventId, tagToUpdate.LapCount, tagToUpdate.TimeLapMs, tagToUpdate.TimeTotalMs, tagToUpdate.PersonId);
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
