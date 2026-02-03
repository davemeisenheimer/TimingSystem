
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Diagnostics;

namespace TrailMeisterDb
{
    public class DbTag {
        private long? _personId;
        public DbTag(int id, string epc, long? personId)
        {
            this.TagId = id;
            this.EPC = epc;
            this.PersonId = personId;
        }
        public int TagId { get; set; }
        public string EPC { get; set; }

        public long? PersonId {
            get
            {
                return this._personId;
            }
            set
            {
                if (this._personId != value)
                {
                    this._personId = value;
                    OnPropertyChanged(nameof(PersonId));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class DbTagFactory : IDbRowItem<DbTag>
    {
        DbTag IDbRowItem<DbTag>.createItem(MySqlDataReader reader)
        {
            var personId = reader["PersonId"];
            if (personId == null || personId == DBNull.Value)
            {
                personId = default(int?); // returns the default value for the type
            }
            return new DbTag(
                Convert.ToInt32(reader["id"]),
                (string)reader["EPC"],
                (int?)personId);
        }
    }


    public class DbTagsTable: DbTable<DbTag>
    {
        public DbTagsTable() : base("tags", new DbTagFactory()) { }

        // Returns all tags in db
        public List<DbTag> getTags()
        {
            Hashtable queryParams = new Hashtable() { };
            return base.getRowItems(queryParams);
        }

        // Will return a tag known to the db or will create a new record for it and return that
        public DbTag? getTag(string epc)
        {
            Hashtable queryParams = new Hashtable() { { "EPC", epc } };
            DbTag? tag = base.getRowItem(queryParams);

            if (tag == null)
            {
                this.addTag(epc, 23);
                tag = base.getRowItem(queryParams);
            }

            return tag;
        }

        public long addTag(string epc, long personId)
        {
            Hashtable columnData = new Hashtable() {
                { "PersonId", personId },
                {"EPC", epc }
            };
            return base.addRow(columnData);
        }

        public void updateTag(long id, long? personId)
        {
            if (personId is long pId) { 
                base.updateColumnValue(id, "PersonId", pId.ToString());
            }
        }
    }
}