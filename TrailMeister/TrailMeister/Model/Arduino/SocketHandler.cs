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
                    handlerSocket.ReceiveTimeout = 31000;

                    IPEndPoint? endPoint = handlerSocket.RemoteEndPoint as IPEndPoint;
                    if (endPoint == null) { Debug.WriteLine("[SOCK] Accepted connection had null endpoint, skipping"); continue; }

                    IPAddress clientAddress = endPoint.Address;
                    Debug.WriteLine($"[SOCK] Connection accepted from {clientAddress}");

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
                                // If the Arduino has already reconnected, a new connection is pending in the backlog.
                                // The old connection is a half-open zombie — abandon it and accept the new one.
                                if (listeningSocket.Poll(0, SelectMode.SelectRead))
                                {
                                    Debug.WriteLine("[SOCK] Stale connection detected — new Arduino connection pending, reconnecting");
                                    break;
                                }
                                continue;
                            }
                        }

                        // bytesRec == 0 means the Arduino closed the connection cleanly
                        if (bytesRec == 0) break;

                        message += Encoding.ASCII.GetString(buffer, 0, bytesRec);

                        // Each Arduino message ends with a blank line (\r\n\r\n).
                        // Process all complete messages individually so that a debug message
                        // and tag data arriving in the same Receive() buffer are handled separately.
                        string terminator = Environment.NewLine + Environment.NewLine;
                        int idx;
                        while ((idx = message.IndexOf(terminator)) >= 0)
                        {
                            string singleMessage = message.Substring(0, idx + terminator.Length);
                            message = message.Substring(idx + terminator.Length);

                            if (singleMessage.Contains(ITagDataSource.END_READY_MESSAGE))
                            {
                                TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.ReaderReady, singleMessage));
                            }
                            else if (singleMessage.Contains(ITagDataSource.END_TAG_DATA))
                            {
                                TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.LapData, singleMessage));
                            }
                            else if (singleMessage.Contains(ITagDataSource.END_DEBUG_MESSAGE))
                            {
                                Debug.WriteLine(singleMessage.Trim());
                            }
                            else if (singleMessage.Contains(ITagDataSource.END_HEARTBEAT_MESSAGE))
                            {
                                // Heartbeat — no action needed, receiving it resets the receive timeout
                            }
                        }
                        Debug.Flush();
                    }

                    // I don't think we can disconnect every time we close the socket. That has the wrong effect on the UI - returns to connection page
                    //this.TagReadEvent?.Invoke(this, new TagDataEventArgs(TagDataSourceEventType.Disconnected, "Client disconnected"));
                    handlerSocket.Close();
                    handlerSocket.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SOCK] Exception in handler: {ex.GetType().Name} {(ex is SocketException se ? se.SocketErrorCode.ToString() : ex.Message)}");
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
