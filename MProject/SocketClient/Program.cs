using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        public static SuperSocket.ClientEngine.AsyncTcpSession client = new SuperSocket.ClientEngine.AsyncTcpSession();
        static void Main(string[] args)
        {
            client.Closed += Client_Closed;
            client.Connected += Client_Connected;
            client.DataReceived += Client_DataReceived;
            client.Error += Client_Error;
            new Thread(() => { Conn(); }).Start();
            while (true)
            {
                Console.ReadLine();
            }
        }

        static void Conn()
        {
            client.Connect(new IPEndPoint(IPAddress.Parse("192.168.192.99"), 8900));
        }

        private static void Client_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Conn();
        }

        private static void Client_DataReceived(object sender, SuperSocket.ClientEngine.DataEventArgs e)
        {
            Console.WriteLine("MSG:" + Encoding.ASCII.GetString(e.Data,0,e.Length));
        }

        private static void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected Server");
        }

        private static void Client_Closed(object sender, EventArgs e)
        {
            Conn();
        }
    }
}
