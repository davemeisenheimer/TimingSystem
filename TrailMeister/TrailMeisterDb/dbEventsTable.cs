using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;

namespace TrailMeisterDb
{
    public class DbEvent : IDbRowItem<DbEvent>
    {
        internal DbEvent(int id, string name, DateOnly date)
        {
            this.ID = id;
            this.EventName = name;
            this.EventDate = date;
        }
        public int ID { get; set; }
        public string EventName { get; set; }
        public DateOnly EventDate { get; set; }

        DbEvent IDbRowItem<DbEvent>.createItem(MySqlDataReader reader)
        {
            return new DbEvent(
                               Convert.ToInt32(reader["id"]),
                               (string)reader["EventName"],
                               (DateOnly)reader["EventDate"]);
        }
    }

    internal class DbEventFactory: IDbRowItem<DbEvent>
    {
        DbEvent IDbRowItem<DbEvent>.createItem(MySqlDataReader reader)
        {
            return new DbEvent(
                               Convert.ToInt32(reader["id"]),
                               (string)reader["EventName"],
                               (DateOnly)reader["EventDate"]);
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

        public long addEvent(string eventName)
        {
            Hashtable columnData = new Hashtable() {
                { "EventName", eventName },
                {"EventDate", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM/dd") }
            };
            return base.addRow(columnData);
        }

        public void updateEvent(long eventId, string eventName)
        {
            base.updateColumnValue(eventId, "EventName", eventName);
        }
    }
}
