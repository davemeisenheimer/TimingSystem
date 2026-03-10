using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using TrailMeisterDb;

namespace TrailMeister.Model.Arduino
{
    internal class SocketClient
    {
        internal static void SendCommand(string command)
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPAddress ipAddress = IPAddress.Parse(AppSettings.Current.ArduinoIpAddress);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, AppSettings.Current.ArduinoPort);
                
                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);
                    sender.ReceiveTimeout = 8000;

                    if (sender.RemoteEndPoint != null)
                    {
                        Debug.WriteLine($"Socket connected to {sender.RemoteEndPoint}");
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
