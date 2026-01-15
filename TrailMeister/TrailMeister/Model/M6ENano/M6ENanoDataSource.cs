using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Data;
using TrailMeister.Model.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using ThingMagic;
using TrailMeisterUtilities;

namespace TrailMeister.Model.M6ENano
{
    internal class M6ENanoDataSource : TrailMeisterUtilities.Disposable, ITagDataSourceConfigurable
    {
        private delegate void ReaderDataItemStateChangeEventHandler(ReaderDataItem sender, TagStateChangedEventArgs eventArgs);
        private M6ENanoConfig _config;

        private class ReaderDataItem: TrailMeisterUtilities.Disposable
        {
            internal event ReaderDataItemStateChangeEventHandler? ReaderDataItemChangeEvent;


            private ReaderData _readerData;
            private TagStateMachine _tagStateMachine;

            internal ReaderDataItem(ReaderData data)
            {
                _readerData = data;
                _tagStateMachine = new TagStateMachine();
                _tagStateMachine.TagStateChangeEvent += OnTagStateChanged;
            }

            public ReaderData Data { 
                get { 
                    return _readerData;
                }
            }

            internal LapState LapState
            {
                get
                {
                    return _tagStateMachine.CurrentState.State;
                }
            }

            // Called when a the tag is read
            // Returns true if the tag data should be sent now
            internal bool updateOnRead(ReaderData data)
            {
                switch(_tagStateMachine.CurrentState.State) {
                    case LapState.DETECT:
                        bool delayElapsed = data.TimeStamp - Data.TimeStamp > _tagStateMachine.CurrentState.DelayToNextState;

                        if (delayElapsed)
                        {
                            // New lap has been detected!  Start gather RSSIs to find the strongest signal
                            _tagStateMachine.MoveToNextState();
                        }
                        break;
                
                    case LapState.GATHERING:
                        if (data.Rssi >= Data.Rssi)
                        {
                            _tagStateMachine.DeferNextStateChange();
                        } else
                        {
                            // We're done!
                            data.Rssi = 0;
                            _tagStateMachine.MoveToNextState(); // Eventing will take it from here
                        }
                        break;
                    case LapState.SENT:
                        _tagStateMachine.DeferNextStateChange();  // Delay moving to the next state
                        break;
                }

                _readerData = data;  // Update our timestamp from the read data
                return false;
            }

            private void OnTagStateChanged(object sender, TagStateChangedEventArgs e)
            {
                if (e.State == LapState.SENT)
                {
                    ReaderDataItemChangeEvent?.Invoke(this, e);
                }
            }
            protected override void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        //dispose managed resources
                        _tagStateMachine.TagStateChangeEvent -= OnTagStateChanged;
                    }
                }
                //dispose unmanaged resources

                _disposed = true;
            }
        }


        // Begin M6ENanoDataSource Class
        public event TagDataSourceEventHandler? TagDataSourceEvent;
        private static object _syncLock = new object();
        private Dictionary<string, ReaderDataItem> _reads = new Dictionary<string, ReaderDataItem>();

        internal M6ENanoDataSource()
        {
            _config = new M6ENanoConfig();
        }

        public void init()
        {
            TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Connecting, "Attempting to find tag reader"));
            Reader? reader = CheckPortsForReader();

            if (reader == null)
            {
                TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Tag reader not found on any USB ports!"));
                return;
            }

            try
            {
                reader.Connect();
            }
            catch (IOException)
            {
                TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Tag reader connect failed!"));
                return;
            }
            catch (ReaderCommException)
            {
                TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Tag reader connect failed!"));
                return;
            }
            catch (UnauthorizedAccessException)
            {
                TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Tag reader connect failed!"));
            }

            TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Connected, "Tag reader connected!"));

            _config.setReader(reader);

            // Region
            string[] list = reader.ParamList();
            Reader.Region[] regions = (Reader.Region[])reader.ParamGet("/reader/region/supportedRegions");
            reader.ParamSet(list[9], Reader.Region.NA2);

            // Antenna
            int[] antennaList;
            string str = "1,1";
            antennaList = Array.ConvertAll<string, int>(str.Split(','), int.Parse);
            SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);
            reader.ParamSet("/reader/read/plan", plan);

            // Single read example
            //int timeout = 1000;
            //TagReadData[] tags = new TagReadData[1];
            //tags = reader.Read(timeout);

            reader.ParamSet("/reader/read/asyncOffTime", 0);
            reader.ParamSet("/reader/tagReadData/recordHighestRssi", true);

            //int powerMax = (int)reader.ParamGet("/reader/radio/powerMax");

            Config.SetAntennaPower(500);
            //reader.ParamSet("/reader/radio/writePower", 0);


            TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.ReaderReady, "Reader is ready"));

            reader.StartReading();
            reader.TagRead += OnTagRead;        
        }

        public ITagReaderConfig Config { get { return _config; } }

        TagReaderDataSourceType ITagDataSource.DataSourceType => TagReaderDataSourceType.M6ENano;

        private void OnTagRead(object? sender, TagReadDataEventArgs e)
        {
            ReaderData tag = new ReaderData(e.TagReadData);

            lock (_syncLock)
            {
                ReaderDataItem? existingDataItem;
                if (_reads.TryGetValue(tag.EPC, out existingDataItem)) {
                    existingDataItem.updateOnRead(tag);
                } else
                {
                    // First time reading this tag
                    ReaderDataItem newItem = new ReaderDataItem(new ReaderData(tag.EPC, tag.TimeStamp, tag.Rssi));
                    newItem.ReaderDataItemChangeEvent += OnTagStateChanged;
                    _reads.Add(tag.EPC, newItem);
                }
            }
        }

        private void OnTagStateChanged(ReaderDataItem data, TagStateChangedEventArgs e)
        {
            if (e.State == LapState.SENT)
            {
                TagDataSourceEvent?.Invoke(
                    this, new TagDataEventArgs(
                        TagDataSourceEventType.LapData,
                        "Tag read from M6ENano",
                        data.Data)
                    );
                BackgroundBeep.Beep(800);
            }
        }

        private Reader? CheckPortsForReader()
        {
            using (var mgmtObject = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                ManagementObjectCollection mgmtObjectList = mgmtObject.Get();

                foreach (ManagementObject manObj in mgmtObjectList)
                {
                    //Debug.WriteLine(manObj["DeviceID"].ToString());
                    //Debug.WriteLine(manObj["Name"].ToString());
                    //Debug.WriteLine(manObj["Caption"].ToString());
                    // If we match this DeviceID: VID_1A86&PID_7523 (USB\\VID_1A86&PID_7523\\5&2DA41B4A&0&1)
                    // Then get the com port from the device name via regex ("USB-SERIAL CH340 (COM9)")
                    // And construct a uri with which to attempt a connection
                    // If the connection succeeds then break out of this loop
                    string? uri = GetUriForPort(manObj);
                    try
                    {
                        if (uri != null) { return Reader.Create(uri); }
                    } catch(FileNotFoundException e) {
                        Debug.WriteLine(String.Format("Failed to create tag reader on {0}; {1}", uri, e.Message));
                    }
                }
            }

            return null;
        }

        private string? GetUriForPort(ManagementObject manObj) { 
            string? deviceId = GetPropertyFromMgmtObject("DeviceID", manObj);

            // If we match this DeviceID: VID_1A86&PID_7523 (USB\\VID_1A86&PID_7523\\5&2DA41B4A&0&1)
            if (deviceId != null && deviceId.Contains("VID_1A86&PID_7523"))
            {
                string? deviceName = GetPropertyFromMgmtObject("Name", manObj);

                if (deviceName != null)
                {
                    // Device name looks like this: USB-SERIAL CH340 (COM9)
                    deviceName = deviceName.ToLower();
                    int startIndex = deviceName.IndexOf("(com") + 1;
                    string portName = deviceName.Substring(startIndex, 4); // Assuming there are no com10s in the world?

                    return string.Format("eapi:///{0}", portName);
                }
            }

            return null;
        }

        private string? GetPropertyFromMgmtObject(string propertyName, ManagementObject obj)
        {
            if (obj[propertyName] != null)
            {
                return obj[propertyName].ToString();
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}
