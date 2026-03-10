using System;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using TrailMeisterUtilities;
using System.Diagnostics;

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
            TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Connecting, "Client connecting"));
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

                    if (!_isListening)
                    {
                        TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Connected, "Client connected: " + clientAddress));
                    }

                    string? message = null;
                    _isListening = true;

                    while (_isListening)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRec = 0;

                        if (handlerSocket.Connected)
                        {
                            try
                            {
                                bytesRec = handlerSocket.Receive(buffer);
                            }
                            catch (SocketException se) when (se.SocketErrorCode == SocketError.TimedOut)
                            {
                                // No data within the receive timeout — Arduino is still connected, keep waiting
                                continue;
                            }
                        }

                        // bytesRec == 0 means the Arduino closed the connection cleanly
                        if (bytesRec == 0) break;

                        message += Encoding.ASCII.GetString(buffer, 0, bytesRec);

                        if (message.EndsWith(Environment.NewLine))
                        {
                            if (message.Contains(ITagDataSource.END_READY_MESSAGE))
                            {
                                TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.ReaderReady, message));
                                message = "";
                            }
                            else if (message.Contains(ITagDataSource.END_TAG_DATA))
                            {
                                TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.LapData, message));
                                message = "";
                            }
                            else if (message.Contains(ITagDataSource.END_DEBUG_MESSAGE))
                            {
                                Debug.WriteLine(message.Trim());
                                message = "";
                            }
                        }
                        Debug.Flush();
                    }

                    // I don't think we can disconnect every time we close the socket. That has the wrong effect on the UI - returns to connection page
                    //this.TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Client disconnected"));
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
                    this.TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Client disconnected"));
                }
            }
            //dispose unmanaged resources
            _isRunning = false;
            _isListening = false;
            _disposed = true;
        }
    }
}
