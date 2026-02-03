using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailMeisterUtilities
{
    public interface IAppDisposable : IDisposable
    {
        string Name { get; }
    }
}
