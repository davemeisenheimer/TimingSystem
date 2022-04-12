
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;

namespace TrailMeisterDb
{
    public class DbTag
    {
        public DbTag(int id, string epc, string humanName)
        {
            this.TagId = id;
            this.EPC = epc;
            this.HumanName = humanName;
        }
        public int TagId { get; set; }
        public string EPC { get; set; }
        public string HumanName { get; set; }
    }

    internal class DbTagFactory : IDbRowItem<DbTag>
    {
        DbTag IDbRowItem<DbTag>.createItem(MySqlDataReader reader)
        {
            return new DbTag(
                Convert.ToInt32(reader["id"]),
                (string)reader["EPC"],
                (string)reader["HumanName"]);
        }
    }


    public class DbTagsTable: DbTable<DbTag>
    {
        public DbTagsTable() : base("tags", new DbTagFactory()) { }

        // Will return a tag known to the db or will create a new record for it and return that
        public DbTag? getTag(string epc)
        {
            Hashtable queryParams = new Hashtable() { { "EPC", epc } };
            DbTag? tag = base.getRowItem(queryParams);

            if (tag == null)
            {
                this.addTag(epc, "Guest");
                tag = base.getRowItem(queryParams);
            }

            return tag;
        }

        public long addTag(string epc, string humanName)
        {
            Hashtable columnData = new Hashtable() {
                { "HumanName", humanName },
                {"EPC", epc }
            };
            return base.addRow(columnData);
        }

        public void updateTag(long id, string name)
        {
            base.updateColumnValue(id, "HumanName", name);
        }
    }
}