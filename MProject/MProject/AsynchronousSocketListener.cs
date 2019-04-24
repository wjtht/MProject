﻿
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MProject
{
    //New Connect
    public delegate void OnNewConnected(StateObject state);
    public delegate void OnReceiveMsg(StateObject state, string msg);
    public delegate void OnClosed(StateObject state);

    // State object for reading client data asynchronously  
    public class StateObject
    {
        public string SocketID = null;
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        public static event OnNewConnected OnConn;
        public static event OnReceiveMsg OnReceive;
        public static event OnClosed OnClose;

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static string key = "\r\n";
        public static int Port = 1100;

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Any;// ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for connection...");
                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.SocketID = System.Guid.NewGuid().ToString();
            state.workSocket = handler;

            OnConn?.Invoke(state);
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    content = state.sb.ToString();
                    while (content.IndexOf(key) > -1)
                    {
                        string tmpStr = content.Substring(0, content.IndexOf(key));
                        content = content.Substring(content.IndexOf(key) + key.Length);
                        OnReceive?.Invoke(state, tmpStr);
                    }
                    state.sb.Clear().Append(content);
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    OnClose?.Invoke(state);
                }
            }
            catch
            {
                try
                {
                    state.workSocket.Shutdown(SocketShutdown.Both);
                    state.workSocket.Close();
                    OnClose?.Invoke(state);
                }
                catch { }
            }
        }

        public static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data + "\r\n");

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;
            try
            {

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                try
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                catch (Exception) { }
            }
        }
    }
}