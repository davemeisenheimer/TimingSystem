using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailMeisterDb
{
    internal interface IDbTable
    {
        string TableName
        {
            get;
        }
    }
}
