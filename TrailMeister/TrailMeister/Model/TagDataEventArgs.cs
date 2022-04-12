using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeister.Model.Data;

namespace TrailMeister.Model
{    public class TagDataEventArgs : EventArgs
    {
        internal TagDataEventArgs(TagDataSourceEventType type, string message)
        {
            switch(type)
            {
                case TagDataSourceEventType.LapData:
                case TagDataSourceEventType.Connected:
                case TagDataSourceEventType.ReaderReady:
                    ReaderStatus = ReaderStatus.Connected;
                    break;
                case TagDataSourceEventType.Connecting:
                    ReaderStatus = ReaderStatus.Connecting;
                    break;
                case TagDataSourceEventType.Disconnected:
                default:
                    ReaderStatus = ReaderStatus.Disconnected;
                    break;

            }
            Type = type;
            Message = message;
        }

        internal TagDataEventArgs(TagDataSourceEventType type, string message, ReaderData data): this(type, message)
        {
            RecentLapData = data;
        }

        public TagDataSourceEventType Type { get; set; }
        public string Message { get; set; }
        public ReaderData? RecentLapData { get; set; }

        public ReaderStatus ReaderStatus { get; set; }
    }
}