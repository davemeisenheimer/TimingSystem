using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailMeister.Model.Arduino
{
    internal class ArduinoConfig: ITagReaderConfig
    {

        public void Reset()
        {
            SocketClient.SendCommand("Reset");
        }

        public void StartReader(int power)
        {
            SetAntennaPower(power);
        }
        public void StopReader()
        {
            SocketClient.SendCommand(String.Format("StopReader"));
        }
        private void SetAntennaPower(int power)
        {
            if (power < 0 || power > 27000) { throw new ArgumentException("power is outside range of 0-27000"); }

            // SetAntennaGain will automatically stop and restart the reader
            SocketClient.SendCommand(String.Format("SetAntennaGain,{0}", power));
        }
    }
}
