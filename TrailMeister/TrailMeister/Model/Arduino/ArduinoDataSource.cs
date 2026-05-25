using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TrailMeister.Model.Data;
using TrailMeisterUtilities;
using System.Windows.Threading;
using System.Diagnostics;

namespace TrailMeister.Model.Arduino
{
    internal sealed class ArduinoDataSource : Disposable, ITagDataSourceConfigurable
    {
        public string Name => "ArduinoDataSource"; // For AppDisposables

        public event TagDataSourceEventHandler? TagDataSourceEvent;

        const int listeningPort = 13000;

        private Socket? _listeningSocket;
        private SocketHandler? _socketHandler;
        private Thread? _serverThread; 
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        private static readonly ArduinoDataSource instance = new ArduinoDataSource();
        private readonly ArduinoConfig _config;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ArduinoDataSource()
        {
        }

        private ArduinoDataSource()
        {
            _config = new ArduinoConfig();
        }

        public void init()
        {
            // Clean up any previous handler so "Poke it!" can retry.
            if (_socketHandler != null)
            {
                _socketHandler.TagReadEvent -= OnTagReadEvent;
                _socketHandler.Dispose();
                _socketHandler = null;
            }

            // Create the listening socket only once — reuse it across retries.
            if (_listeningSocket == null)
            {
                IPAddress listeningIp = IPAddress.Any;
                IPEndPoint listeningEndPoint = new IPEndPoint(listeningIp, listeningPort);
                _listeningSocket = new Socket(listeningIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listeningSocket.Bind(listeningEndPoint);
                _listeningSocket.Listen(10);
            }

            _socketHandler = new SocketHandler(_listeningSocket);
            _socketHandler.TagReadEvent += OnTagReadEvent;
            _serverThread = new Thread(_socketHandler.startListening)
            {
                IsBackground = true
            };
            _serverThread.Start();
        }

        public ITagReaderConfig Config {
            get { return _config; } 
        }

        internal static ArduinoDataSource Instance
        {
            get
            {
                return instance;
            }
        }

        public TagReaderDataSourceType DataSourceType
        {
            get
            {
                return TagReaderDataSourceType.Arduino;
            }
        }

        internal SocketHandler? SocketHandler
        {
            get
            {
                return this._socketHandler;
            }
        }

        private void OnTagReadEvent(object sender, TagDataEventArgs e)
        {
            _dispatcher.BeginInvoke(() =>
            {
                if (e.Type == TagDataSourceEventType.LapData)
                {
                    List<ReaderData> data = TagDataArduino.getReaderData(e.Message);
                    foreach (ReaderData d in data)
                    {
                        TagDataSourceEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.LapData, e.Message, d));
                    }
                } else
                {
                    TagDataSourceEvent?.Invoke(this, e);
                }
            });
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
            }
            //dispose unmanaged resources
            // Best-effort shutdown commands — short timeout so closing the app stays fast.
            try { _config.StopReader(connectTimeoutMs: 500); } catch (SocketException) { }
            try { _config.Reset(connectTimeoutMs: 500); } catch (SocketException) { }

            if (_socketHandler != null)
            {
                _socketHandler.TagReadEvent -= OnTagReadEvent;
                this._socketHandler.Dispose();
                while (_socketHandler.IsRunning);
            }
            this._socketHandler = null;

            if (_listeningSocket != null)
            {
                _listeningSocket.Close();
                _listeningSocket.Dispose();
            }

            if (_serverThread != null)
            {
                this._serverThread.Interrupt();
            }

            _disposed = true;
        }
    }
}
