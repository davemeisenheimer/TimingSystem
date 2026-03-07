
using MySql.Data.MySqlClient;
using System.Collections;
using System.ComponentModel;

namespace TrailMeisterDb
{
    public class DbEvent : IDbRowItem<DbEvent>
    {
        private string _eventName;
        private int _lapLength;
        private bool _eventFinished;
        internal DbEvent(uint id, string name, int lapLength, DateTime date, bool eventFinished)
        {
            this.ID = id;
            this._eventName = name;
            this.LapLength = lapLength;
            this.EventDate = date;
            this.EventFinished = eventFinished;
        }
        public uint ID { get; set; }
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
        public int LapLength
        {
            get
            {
                return this._lapLength;
            }
            set
            {
                if (this._lapLength != value)
                {
                    this._lapLength = value;
                    OnPropertyChanged(nameof(LapLength));
                }
            }
        }
        public DateTime EventDate { get; set; }

        public bool EventFinished
        {
            get
            {
                return this._eventFinished;
            }
            set
            {
                if (this._eventFinished != value)
                {
                    this._eventFinished = value;
                    OnPropertyChanged(nameof(EventFinished));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        DbEvent IDbRowItem<DbEvent>.createItem(MySqlDataReader reader)
        {
            return new DbEvent(
                               Convert.ToUInt32(reader["id"]),
                               (string)reader["EventName"],
                               (int)reader["LapLength"],
                               (DateTime)reader["EventDate"],
                               Convert.ToBoolean(reader["EventFinished"]));
        }
    }

    internal class DbEventFactory: IDbRowItem<DbEvent>
    {
        DbEvent IDbRowItem<DbEvent>.createItem(MySqlDataReader reader)
        {
            return new DbEvent(
                               Convert.ToUInt32(reader["id"]),
                               (string)reader["EventName"],
                               (int)reader["LapLength"],
                               (DateTime)reader["EventDate"],
                               Convert.ToBoolean(reader["EventFinished"]));
        }
    }

    public class DbEventsTable : DbTable<DbEvent>
    {
        public DbEventsTable() : base("events", new DbEventFactory()) {
        }

        // Could push these privates to a base class
        DbEventFactory dbEventFactory = new DbEventFactory();

        public DbEvent? getEvent(uint id)
        {
            return base.getRowItem(id);
        }
        public List<DbEvent> getEvents()
        {
            Hashtable queryParams = new Hashtable() { };
            return base.getRowItems(queryParams);
        }
        public List<DbEvent> getEventsByIds(List<long>? idList)
        {
            if (idList == null || !idList.Any()) return new List<DbEvent>();

            // Create a string like "1, 2, 3"
            string joinedIds = string.Join(", ", idList);

            Hashtable queryParams = new Hashtable();

            // We craft the Key so the final string becomes:
            // WHERE EventId IN (1, 2, 3) AND 1 = 1
            // The "1" at the end comes from the 'Value' part of the Hashtable
            queryParams.Add($"ID IN ({joinedIds}) AND 1", "1");

            return base.getRowItems(queryParams);
        }

        public long addEvent(string eventName, int lapLength)
        {
            Hashtable columnData = new Hashtable() {
                { "EventName", eventName },
                { "LapLength", lapLength },
                {"EventDate", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM/dd") }
            };
            return base.addRow(columnData);
        }
        public void deleteEvent(long eventId)
        {
            base.deleteRow(eventId);
        }

        public void updateEvent(long eventId, string eventName, int lapLength, bool isFinished = false)
        {
            base.updateColumnValue(eventId, "EventName", eventName);
            base.updateColumnValue(eventId, "LapLength", lapLength.ToString());
            base.updateColumnValue(eventId, "EventFinished", isFinished ? "1" : "0");
        }
    }
}
