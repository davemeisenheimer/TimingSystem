using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThingMagic;

namespace TrailMeister.Model.M6ENano
{
    internal class M6ENanoConfig: ITagReaderConfig
    {
        private Reader? _reader;

        internal M6ENanoConfig()
        {
        }

        internal M6ENanoConfig(Reader reader)
        {
            _reader = reader;
        }

        internal void setReader(Reader reader)
        {
            this._reader = reader;
        }

        public bool SetAntennaPower(int power)
        {
            if (power < 0 || power > 2700) { throw new ArgumentException("power is outside range of 0-2700"); }
            if (_reader == null)
            {
                return false;
            }

            _reader.ParamSet("/reader/radio/readPower", power);
            
            return true;
        }
    }
}
