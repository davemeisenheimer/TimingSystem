using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Text.Json;

namespace TrailMeisterDb
{
    public class DbLap
    {
        internal DbLap(long lapId, long tagId, long eventId, uint lapCount, ulong lapTime, ulong totalTime, long personId)
        {
            this.LapId = lapId;
            this.TagId = tagId;
            this.EventId = eventId;
            this.LapCount = lapCount;
            this.LapTime = lapTime;
            this.TotalTime = totalTime;
            this.PersonId = personId;
        }
        public long LapId { get; set; }
        public long TagId { get; set; }
        public long EventId { get; set; }
        public uint LapCount { get; set; }
        public ulong LapTime { get; set; }
        public ulong TotalTime { get; set; }
        public long PersonId { get; set; }
    }

    internal class DbLapFactory : IDbRowItem<DbLap>
    {
        DbLap IDbRowItem<DbLap>.createItem(MySqlDataReader reader)
        {
            return new DbLap(
                            Convert.ToInt32(reader["id"]),
                            Convert.ToInt32(reader["tagId"]),
                            Convert.ToInt32(reader["eventId"]),
                            Convert.ToUInt32(reader["LapCount"]),
                            Convert.ToUInt64(reader["LapTime"]),
                            Convert.ToUInt64(reader["TotalTime"]),
                            Convert.ToInt32(reader["PersonId"]));
        }
    }
    public class DbLapsTable: DbTable<DbLap>
    {
        public DbLapsTable() : base("laps", new DbLapFactory()) { }


        public void deleteEventLapsForEvent(long eventId)
        {
            List<DbLap> dbLaps = this.getEventLapsForEvent(eventId);
            foreach(DbLap dbLap in dbLaps)
            {
                base.deleteRow(dbLap.LapId);
            }
        }


        public List<DbLap>? getEventLapsForEvent(long eventId)
        {
            Hashtable queryParams = new Hashtable() { { "eventId", eventId } };

            return base.getRowItems(queryParams);
        }

        public List<DbLap>? getEventLapsForRacer(long tagId, long eventId)
        {
            Hashtable queryParams = new Hashtable() { { "tagId", tagId }, { "eventId", eventId } };

            return base.getRowItems(queryParams);
        }

        public List<DbLap>? getAllLapsForRacer(long personId)
        {
            Hashtable queryParams = new Hashtable() { { "PersonId", personId } };

            return base.getRowItems(queryParams);
        }
        public void addLap(int tagId, long eventId, uint lapCount, ulong lapTime, ulong totalTime, long? personId)
        {
            Hashtable columnData = new Hashtable() {
                { "tagId", tagId },
                {"eventId", eventId },
                {"LapCount", lapCount },
                {"LapTime", lapTime },
                {"TotalTime", totalTime },
                {"PersonId", personId }
            };

            try
            {
                base.addRow(columnData);
            } catch (Exception ex)
            {
                string lapDataJson = JsonSerializer.Serialize(columnData, new JsonSerializerOptions { WriteIndented = true });
                string msg = "Tried to add lap data for event, but something went wrong. " + lapDataJson;
                throw new Exception(msg, ex);   
            }
        }
    }
}
