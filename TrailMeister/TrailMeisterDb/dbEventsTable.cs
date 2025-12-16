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
        internal DbEvent(int id, string name, DateTime date)
        {
            this.ID = id;
            this.EventName = name;
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

        public long addEvent(string eventName)
        {
            Hashtable columnData = new Hashtable() {
                { "EventName", eventName },
                {"EventDate", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM/dd") }
            };
            return base.addRow(columnData);
        }
        public void deleteEvent(long eventId)
        {
            base.deleteRow(eventId);
        }

        public void updateEvent(long eventId, string eventName, bool isFinished = false)
        {
            base.updateColumnValue(eventId, "EventName", eventName);
            base.updateColumnValue(eventId, "EventFinished", isFinished ? "1" : "0");
        }
    }
}
