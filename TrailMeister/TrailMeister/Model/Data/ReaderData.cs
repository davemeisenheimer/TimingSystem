using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThingMagic;

namespace TrailMeister.Model.Data
{
    public class ReaderData
    {
        private ReaderData(string epc)
        {
            EPC = epc;
        }

        // hmmm... this is a terrible case to long, but since the arduino always resets to zero on reboot, we should be fine
        internal ReaderData(string epc, ulong timeStamp): this(epc, DateTimeOffset.FromUnixTimeMilliseconds((long) timeStamp).UtcDateTime, null)
        {
        }
        internal ReaderData(string epc, DateTime timeStamp, int? rssi): this(epc)
        {
            TimeStamp = timeStamp;
            Rssi = rssi;
        }

        internal ReaderData(TagReadData tag): this(tag.EpcString, tag.Time, tag.Rssi)
        {
        }

        public string EPC { get; set; } 

        public DateTime TimeStamp { get; set; }    

        public int? Rssi { get; set; }
    }
}
