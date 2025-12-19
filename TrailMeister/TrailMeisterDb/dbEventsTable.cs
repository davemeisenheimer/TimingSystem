using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.ComponentModel;

namespace TrailMeisterDb
{
    public class DbEvent : IDbRowItem<DbEvent>
    {
        private string _eventName;
        private int _lapLength;
        internal DbEvent(int id, string name, int lapLength, DateTime date)
        {
            this.ID = id;
            this.EventName = name;
            this.LapLength = lapLength;
            this.EventDate = date;
        }
        public int ID { get; set; }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        DbEvent IDbRowItem<DbEvent>.createItem(MySqlDataReader reader)
        {
            return new DbEvent(
                               Convert.ToInt32(reader["id"]),
                               (string)reader["EventName"],
                               (int)reader["LapLength"],
                               (DateTime)reader["EventDate"]);
        }
    }

    internal class DbEventFactory: IDbRowItem<DbEvent>
    {
        DbEvent IDbRowItem<DbEvent>.createItem(MySqlDataReader reader)
        {
            return new DbEvent(
                               Convert.ToInt32(reader["id"]),
                               (string)reader["EventName"],
                               (int)reader["LapLength"],
                               (DateTime)reader["EventDate"]);
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
        public List<DbEvent>? getEvents()
        {
            Hashtable queryParams = new Hashtable() { };
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
