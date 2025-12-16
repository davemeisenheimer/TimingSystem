using System;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using TrailMeisterUtilities;

namespace TrailMeister.Model.Arduino {

    internal class SocketHandler: Disposable
    {
        public event TagDataSourceEventHandler? TagReadEvent;

        private Socket listeningSocket;
        private bool _isRunning = true;
        private bool _isListening = false;

        internal SocketHandler(Socket listeningSocket)
        {
            this.listeningSocket = listeningSocket;
        }

        internal bool IsRunning {  get { return _isRunning; } }

        internal void startListening()
        {
            Socket? handlerSocket = null;
            while (true)
            {
                if (!_isRunning) break;

                try
                {
                    // Wait for incoming connection
                    handlerSocket = listeningSocket.Accept();

                    if (handlerSocket == null) { break; }
                    handlerSocket.ReceiveTimeout = 5000;

                    IPEndPoint? endPoint = handlerSocket.RemoteEndPoint as IPEndPoint;
                    if (endPoint == null) { break; }

                    IPAddress clientAddress = endPoint.Address;

                    TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Connected, "Client connected: " + clientAddress));

                    string? message = null;
                    _isListening = true;

                    while (_isListening)
                    {
                        // Save received bytes
                        byte[] buffer = new byte[1024];
                        int bytesRec = 0;

                        //if (handlerSocket.Connected && listeningSocket.Connected)
                        if (handlerSocket.Connected)
                        {
                            bytesRec = handlerSocket.Receive(buffer);
                        }

                        message += Encoding.ASCII.GetString(buffer, 0, bytesRec);

                        // Message completed? Parse it...
                        if (message.Contains(ITagDataSource.END_READY_MESSAGE))
                        {
                            TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.ReaderReady, message));
                            break;
                        }
                        else if (message.Contains(ITagDataSource.END_TAG_DATA))
                        {
                            TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.LapData, message));
                            break;
                        }
                    }

                    this.TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Client disconnected"));
                    handlerSocket.Close();
                    handlerSocket.Dispose();
                }
                catch (Exception)
                {
                   TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Client disconnected due to exception"));
                }
            }

            if (handlerSocket != null)
            {
                handlerSocket.Close();
                handlerSocket.Dispose();
            }
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
            _isRunning = false;
            _isListening = false;
            _disposed = true;
        }
    }
}
