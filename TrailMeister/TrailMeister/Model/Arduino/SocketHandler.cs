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
                        // Save received bytes
                        byte[] buffer = new byte[1024];
                        int bytesRec = 0;

                        //if (handlerSocket.Connected && listeningSocket.Connected)
                        if (handlerSocket.Connected)
                        {
                            bytesRec = handlerSocket.Receive(buffer);

                            //if (bytesRec > 0) Debug.WriteLine("bytesRec: " + bytesRec);
                        }

                        message += Encoding.ASCII.GetString(buffer, 0, bytesRec);

                        if (message.EndsWith(Environment.NewLine))
                        {
                            // Message completed? Parse it...
                            
                            if (message.Contains(ITagDataSource.END_READY_MESSAGE))
                            {
                                //Debug.WriteLine("Socket message received: " + message);
                                TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.ReaderReady, message));
                                message = "";
                                break;
                            }
                            else if (message.Contains(ITagDataSource.END_TAG_DATA))
                            {
                                //Debug.WriteLine("Socket message received: " + message);
                                TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.LapData, message));
                                message = "";
                                break;
                            }
                            else if (message.Contains(ITagDataSource.END_DEBUG_MESSAGE))
                            {
                                Debug.WriteLine(message.Trim());
                                break;
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
