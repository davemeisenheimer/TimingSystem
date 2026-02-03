using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeisterUtilities;

namespace TrailMeister.Model
{
    public enum TagReaderDataSourceType
    {
        Arduino,
        M6ENano
    };
    
    public enum TagDataSourceEventType
    {
        Connected,
        Disconnected,
        Connecting,
        LapData,
        ReaderReady
    }

    public enum ReaderStatus
    {
        Connected,
        Connecting,
        Disconnected
    };

    public delegate void TagDataSourceEventHandler(object sender, TagDataEventArgs eventArgs);
    internal interface ITagDataSource: IDisposable, IAppDisposable
    {
        const string END_TAG_DATA = "TAG_DATA_##$$##";
        const string END_READY_MESSAGE = "READER_READY_##$$##";
        const string END_DEBUG_MESSAGE = "DEBUG_##$$##";

        internal TagReaderDataSourceType DataSourceType { get; }

        public void init();

        public event TagDataSourceEventHandler TagDataSourceEvent;

        public ITagReaderConfig Config { get; }
    }
}
