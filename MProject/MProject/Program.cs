using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MProject
{
    class Program
    {
        static void Main(string[] args)
        {
            AsynchronousSocketListener.OnConn += AsynchronousSocketListener_OnConn;
            AsynchronousSocketListener.OnReceive += AsynchronousSocketListener_OnReceive;
            AsynchronousSocketListener.OnClose += AsynchronousSocketListener_OnClose;
            AsynchronousSocketListener.Port = 8900;
            AsynchronousSocketListener.StartListening();
        }

        private static void AsynchronousSocketListener_OnClose(StateObject state)
        {
            Console.WriteLine(state.SocketID + ":Offline");
        }

        private static void AsynchronousSocketListener_OnReceive(StateObject state, string msg)
        {
            Console.WriteLine(state.SocketID + "=>" + msg);
        }

        private static void AsynchronousSocketListener_OnConn(StateObject state)
        {
            Console.WriteLine(state.SocketID + ":Online");
            AsynchronousSocketListener.Send(state.workSocket, state.SocketID);
        }
    }
}
