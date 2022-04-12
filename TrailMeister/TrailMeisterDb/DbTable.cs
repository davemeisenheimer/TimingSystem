using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using MySql.Data.MySqlClient;

namespace TrailMeisterDb
{
    public class DbTable<DbItemType> : IDbTable
    {
        DbProcess dbProcess = new DbProcess();
        string _tableName;
        IDbRowItem<DbItemType> _factory;

        internal DbTable(string tableName, IDbRowItem<DbItemType> factory)
        {
            _tableName = tableName;
            _factory = factory;
        }

        public string TableName { get { return _tableName; } }

        protected void updateColumnValue(long rowId, string columnName, string columnValue)
        {
            string cmdStr = string.Format("UPDATE {0} SET {1} = '{2}' WHERE id = {3}", _tableName, columnName, columnValue, rowId);
            dbProcess.update(cmdStr);
        }
        protected long addRow(Hashtable columnData)
        {
            int columnCount = 0;
            string cmdStr = string.Format("INSERT INTO {0} (", _tableName);
            
            foreach(string key in columnData.Keys)
            {
                cmdStr += columnCount > 0 ? ", ": "";
                cmdStr += key;
                columnCount++;
            }

            cmdStr += ") VALUES (";

            columnCount = 0;
            foreach (var value in columnData.Values)
            {
                cmdStr += columnCount > 0 ? ", ": "";
                cmdStr += "'" + value.ToString() + "'";
                columnCount++;
            }
            cmdStr += ")";

            return dbProcess.add(cmdStr);
        }

        protected DbItemType? getRowItem(long id)
        {
            return dbProcess.getRowItem<DbItemType>(_tableName, id, _factory);
        }

        protected DbItemType? getRowItem(Hashtable searchParams)
        {
            return dbProcess.getRowItem<DbItemType>(_tableName, searchParams, _factory);
        }

        protected List<DbItemType>? getRowItems(Hashtable searchParams)
        {
            return dbProcess.getRowItems<DbItemType>(_tableName, searchParams, _factory);
        }
    }
}
