/* Database connection class*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace TrailMeisterDb
{
    // make this class abstract so that only dbProcess and dbUser class can access this class
    public abstract class dbConnection
    {
        public MySqlConnection _conn;
        public MySqlTransaction? _transaction;

        public dbConnection()
        {
            //string strProject = "ERP001\\SQLR2"; //Enter your SQL server instance name
            //string strProject = "Local instance MySQL80"; //Enter your SQL server instance name
            string strDatabase = "skimeister"; //Enter your database name
            string strUserID = "skimeister "; // Enter your SQL Server User Name
            string strPassword = "@123Cole"; // Enter your SQL Server Password
            string strconn = "Server=localhost" + ";Persist Security Info=false;database=" + strDatabase + ";user id=" + strUserID + ";password=" + strPassword + ";Connection Timeout = 2";
            //string strconn = "data source=" + strProject + ";Persist Security Info=false;database=" + strDatabase + ";user id=" + strUserID + ";password=" + strPassword + ";Connection Timeout = 0";
            _conn = new MySqlConnection(strconn);
        }

        public void openConnection()
        {
            _conn.Close();
            _conn.Open();
            _transaction = _conn.BeginTransaction();
        }

        public void closeConnection()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }
            _conn.Close();
        }

        public void errorTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
            }
            _conn.Close();
        }

        protected void ExecuteSQL(string sSQL)
        {
            openConnection();

            MySqlCommand cmd = new MySqlCommand(sSQL, _conn, _transaction);
            cmd.ExecuteNonQuery();

            closeConnection();
        }

        protected void OnlyExecuteSQL(string sSQL)
        {
            MySqlCommand cmd = new MySqlCommand(sSQL, _conn);
            cmd.ExecuteNonQuery();
        }

        protected DataSet FillDataSet(DataSet dset, string sSQL, string tbl)
        {
            MySqlCommand cmd = new MySqlCommand(sSQL, _conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

            try
            {
                adapter.Fill(dset, tbl);
            }
            finally
            {
                _conn.Close();
            }
            return dset;

        }

        protected DataSet FillData(string sSQL, string sTable)
        {
            MySqlCommand cmd = new MySqlCommand(sSQL, _conn, _transaction);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds, sTable);
            return ds;
        }

        protected MySqlDataReader setDataReader(string sSQL)
        {
            MySqlCommand cmd = new MySqlCommand(sSQL, _conn, _transaction);
            cmd.CommandTimeout = 300;
            MySqlDataReader rtnReader;
            rtnReader = cmd.ExecuteReader();
            return rtnReader;
        }
    }
}