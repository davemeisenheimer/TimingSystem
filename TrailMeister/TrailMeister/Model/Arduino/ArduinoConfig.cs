using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailMeister.Model.Arduino
{
    internal class ArduinoConfig: ITagReaderConfig
    {
        public bool SetAntennaPower(int power)
        {
            if (power < 0 || power > 27000) { throw new ArgumentException("power is outside range of 0-27000"); }

            SocketClient.SendCommand(String.Format("SetAntennaGain,{0}", power));

            return true;
        }
    }
}
