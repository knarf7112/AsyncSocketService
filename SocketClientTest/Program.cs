using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketClientTest
{
    class Program
    {

        static void Main(string[] args)
        {
            //ConsoleKeyInfo key = Console.ReadKey();
            
            ClsSockteClient client = new ClsSockteClient();
            client.ConnectToServer();

            Console.ReadKey();
        }
    }
}
