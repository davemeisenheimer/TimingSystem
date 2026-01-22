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

        internal Reader Reader { get { return _reader; } }

        internal void setReader(Reader reader)
        {
            this._reader = reader;
        }

        public bool StartReader(int antennaPower)
        {
            if (antennaPower < 0 || antennaPower > 2700) { throw new ArgumentException("power is outside range of 0-2700"); }
            if (_reader == null)
            {
                return false;
            }

            _reader.ParamSet("/reader/radio/readPower", antennaPower);
            _reader.ParamSet("/reader/radio/writePower", antennaPower); 
            _reader.StartReading();

            return true;
        }

        public void StopReader()
        {
            _reader.StopReading();
            _reader.Dispose();
        }
    }
}
