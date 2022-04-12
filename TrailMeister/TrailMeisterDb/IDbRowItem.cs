using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TrailMeisterDb
{
    internal interface IDbRowItem<T>
    {
        internal T createItem(MySqlDataReader reader);
    }
}
