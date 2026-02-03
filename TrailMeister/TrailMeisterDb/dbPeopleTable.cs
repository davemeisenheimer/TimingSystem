
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
    public class DbPerson
    {
        private string _firstName = "";
        private string _lastName = "";
        private string _nickName = "";
        private string _association = "";
        public DbPerson(long id, string first, string last = "", string nickname = "", string association = "")
        {
            this.PersonId = id;
            this.FirstName = first;
            this.LastName = last;
            this.NickName = nickname;
            this.Association = association;
        }
        public long PersonId { get; set; }
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                if (this._firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                if (this._lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }
        public string NickName
        {
            get
            {
                return _nickName;
            }
            set
            {
                if (this._nickName != value)
                {
                    _nickName = value;
                    OnPropertyChanged(nameof(NickName));
                }
            }
        }
        public string Association
        {
            get
            {
                return _association;
            }
            set
            {
                if (this._association != value)
                {
                    _association = value;
                    OnPropertyChanged(nameof(Association));
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

    internal class DbPersonFactory : IDbRowItem<DbPerson>
    {
        DbPerson IDbRowItem<DbPerson>.createItem(MySqlDataReader reader)
        {
            return new DbPerson(
                Convert.ToUInt32(reader["id"]),
                (string)reader["firstName"],
                (string)reader["lastName"],
                (string)reader["nickName"],
                (string)reader["association"]);
        }
    }


    public class DbPeopleTable: DbTable<DbPerson>
    {
        public DbPeopleTable() : base("people", new DbPersonFactory()) { }

        // Returns all tags in db
        public List<DbPerson> getPeople()
        {
            Hashtable queryParams = new Hashtable() { };
            return base.getRowItems(queryParams);
        }

        // Will return a tag known to the db or will create a new record for it and return that
        public DbPerson? getPerson(long id)
        {
            Hashtable queryParams = new Hashtable() { { "id", id } };
            DbPerson? person = base.getRowItem(queryParams);

            return person;
        }

        public long addPerson(string firstName, string lastName, string nickName, string association)
        {
            Hashtable columnData = new Hashtable() {
                { "firstName", firstName},
                {"lastName", lastName },
                {"nickName", nickName },
                {"association", association }
            };
            return base.addRow(columnData);
        }

        public void updatePerson(long id, string firstName, string lastName, string nickName, string association)
        {
            base.updateColumnValue(id, "firstName", firstName);
            base.updateColumnValue(id, "lastName", lastName);
            base.updateColumnValue(id, "nickName", nickName);
            base.updateColumnValue(id, "association", association);
        }
    }
}