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
    public class DbLap
    {
        internal DbLap(int lapId, int tagId, int eventId, uint lapCount, ulong lapTime, ulong totalTime, string humanName)
        {
            this.LapId = lapId;
            this.TagId = tagId;
            this.EventId = eventId;
            this.LapCount = lapCount;
            this.LapTime = lapTime;
            this.TotalTime = totalTime;
            this.HumanName = humanName;
        }
        public int LapId { get; set; }
        public int TagId { get; set; }
        public int EventId { get; set; }
        public uint LapCount { get; set; }
        public ulong LapTime { get; set; }
        public ulong TotalTime { get; set; }
        public string HumanName { get; set; }
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
                            (string)reader["HumanName"]);
        }
    }
    public class DbLapsTable: DbTable<DbLap>
    {
        public DbLapsTable() : base("laps", new DbLapFactory()) { }

        //public List<DbLap> getLapsForRacer(string epc)
        //{

        //}
        //public List<DbLap> getLapsForRacer(int tagId)
        //{

        //}
        //public List<DbLap> getEventLapsForRacer(string epc)
        //{

        //}

        public List<DbLap>? getEventLapsForRacer(int tagId)
        {
            Hashtable queryParams = new Hashtable() { { "tagId", tagId } };

            return base.getRowItems(queryParams);
        }

        public void addLap(int tagId, long eventId, uint lapCount, ulong lapTime, ulong totalTime, string humanName)
        {
            Hashtable columnData = new Hashtable() {
                { "tagId", tagId },
                {"eventId", eventId },
                {"LapCount", lapCount },
                {"LapTime", lapTime },
                {"TotalTime", totalTime },
                {"HumanName", humanName }
            };
            base.addRow(columnData);
        }
    }
}
