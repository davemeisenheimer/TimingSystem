/* usefull fuction for modify and query Database */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;

namespace TrailMeisterDb 
{
    class DbProcess : dbConnection
    {
        public MySqlCommand getTextCommand(string command)
        {
            MySqlCommand cmd = new MySqlCommand();
            openConnection();
            cmd.CommandText = command;
            cmd.Connection = _conn;
            cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }

        public void update(string updateCommand)
        {
            using (MySqlCommand cmd = getTextCommand(updateCommand))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    errorTransaction();
                    throw new ApplicationException("Error updating db with command: " + updateCommand, ex);
                }
                finally
                {
                    closeConnection();
                }
            }
        }

        public long add(string addCommand)
        {
            using (MySqlCommand cmd = getTextCommand(addCommand))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                    return cmd.LastInsertedId;
                }
                catch (Exception ex)
                {
                    errorTransaction();
                    throw new ApplicationException("Error updating db with command: " + addCommand, ex);
                }
                finally
                {
                    closeConnection();
                }
            }
        }
        public T? getRowItem<T>(string tableName, Hashtable searchParams, IDbRowItem<T> factory)
        {
            int paramCount = 0;
            string queryParams = "";
            foreach (DictionaryEntry param in searchParams)
            {
                if (paramCount > 1)
                {
                    queryParams += " AND ";
                }
                else
                {
                    queryParams = " where ";
                }

                queryParams += param.Key + " = '" + param.Value + "'";
                paramCount++;
            }

            using (MySqlCommand cmd = new MySqlCommand())
            {
                openConnection();
                MySqlDataReader? conReader;
                conReader = null;
                cmd.CommandText = "Select * from " + tableName + queryParams;
                cmd.Connection = _conn;
                cmd.Transaction = _transaction;
                cmd.CommandType = CommandType.Text;
                //cmd.Parameters.Add("@EPC", MySqlDbType.Int64).Value = id;

                try
                {
                    conReader = cmd.ExecuteReader();

                    while (conReader.Read())
                    {
                        return factory.createItem(conReader);
                    }
                }
                catch (Exception ex)
                {
                    errorTransaction();
                    throw new ApplicationException("Error reading event from db :", ex);
                }
                finally
                {
                    if (conReader != null) { 
                        conReader.Close();
                    }
                    closeConnection();
                }
                return default(T);
            }
        }

        public T? getRowItem<T>(string tableName, long id, IDbRowItem<T> factory)
        {
            return getRowItem<T>(tableName, new Hashtable()
                {
                    {"id", id.ToString()}
                },
                factory);
        }
        public List<T> getRowItems<T>(string tableName, Hashtable searchParams, IDbRowItem<T> factory)
        {
            List<T> list = new List<T>();
            int paramCount = 0;
            string queryParams = "";
            foreach (DictionaryEntry param in searchParams)
            {
                if (paramCount > 1)
                {
                    queryParams += " AND ";
                } else
                {
                    queryParams = " where ";
                }

                queryParams += param.Key + " = " + param.Value;
                paramCount++;
            }

            using (MySqlCommand cmd = new MySqlCommand())
            {
                openConnection();
                MySqlDataReader? conReader;
                conReader = null;
                cmd.CommandText = "Select * from " + tableName + queryParams;
                cmd.Connection = _conn;
                cmd.Transaction = _transaction;
                cmd.CommandType = CommandType.Text;
                //cmd.Parameters.Add("@EPC", MySqlDbType.Int64).Value = id;

                try
                {
                    conReader = cmd.ExecuteReader();

                    while (conReader.Read())
                    {
                        list.Add(factory.createItem(conReader));
                    }
                    return list;
                }
                catch (Exception ex)
                {
                    errorTransaction();
                    throw new ApplicationException("Error reading event from db :", ex);
                }
                finally
                {
                    if (conReader != null)
                    {
                        conReader.Close();
                    }
                    closeConnection();
                }
            }
        }

        public bool isRecordExists(string fldName, string tblName, string param)
        {
            string sSQL = "SELECT " + fldName + " From " + tblName + " WHERE " + fldName + "= '" + param + "'";
            MySqlDataReader dr = setDataReader(sSQL);
            dr.Read();
            bool isExists = ((dr.HasRows == true) ? true : false);
            dr.Close();
            dr.Dispose();
            return isExists;
        }

        public MySqlDataReader getFields(string fldName, string tblName, string condition)
        {
            string sSQL = "SELECT " + fldName + " FROM " + tblName + " " + condition;
            return setDataReader(sSQL);
        }

        public void addRecord(string tblName, string values)
        {
            string sSQL = "INSERT INTO " + tblName + " VALUES(" + values + ")";
            ExecuteSQL(sSQL);
        }

        public void UpdateRecord(string tblName, string values)
        {
            string sSQL = "UPDATE " + tblName + " SET " + values;
            ExecuteSQL(sSQL);
        }

        public void DeleteRecord(string tblName, long id)
        {
            string sSQL = "DELETE FROM " + tblName + " WHERE id = " + id.ToString();
            ExecuteSQL(sSQL);
        }

        public void executeSP(string SPName, string condition)
        {
            string sSQL = "EXEC " + SPName + " " + condition;
            ExecuteSQL(sSQL);
        }

        public MySqlDataReader getSP_Record(string SPName, string condition)
        {
            string sSQL = "EXEC " + SPName + " " + condition;
            return setDataReader(sSQL);
        }

        public void finishSave()
        {
            closeConnection();
            //MessageBox.Show("Data Saved Successfully.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public string getValue(string val, string tbl, string condition)
        {
            string sSQL = "SELECT " + val + " FROM " + tbl + " WHERE " + condition;
            MySqlDataReader dr = setDataReader(sSQL);
            string sValue = "";
            dr.Read();
            if (dr.HasRows == true)
            {
                string? drStr = dr[0].ToString();

                sValue = ((drStr == null || drStr.Trim() == "Null" || drStr.Trim() == "") ? "" : drStr.Trim()); ;
            }
            else sValue = "";
            dr.Close();
            dr.Dispose();
            return sValue;
        }

        public DataSet FillDataWithOpenConn(string sSQL, string sTable)
        {
            return FillData(sSQL, sTable);
        }

    }
}