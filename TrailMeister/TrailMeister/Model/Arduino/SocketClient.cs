using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TrailMeister.Model.Arduino
{
    internal class SocketClient
    {
        internal static IPAddress RemoteIP = new IPAddress(new byte[] { 192, 168, 0, 94 });
        internal static int RemotePort = 13001;

        internal static void SendCommand(string command)
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPAddress ipAddress = RemoteIP;
                IPEndPoint remoteEP = new IPEndPoint(RemoteIP, RemotePort);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);

                    if (sender.RemoteEndPoint != null)
                    {
                        Debug.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                    }

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes(command + "\n\n");

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    Debug.WriteLine(String.Format("Arduino response = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec)));

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Debug.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Debug.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
