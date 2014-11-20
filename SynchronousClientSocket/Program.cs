using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SynchronousSocketClient
{
    public static int Main(String[] args)
    {
        SynchronousClientSocket.SynchronousClientSocket.StartClient();
        return 0;
    }
}