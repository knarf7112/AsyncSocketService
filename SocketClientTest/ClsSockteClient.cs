using System;
using System.Net;
using System.Net.Sockets;
namespace SocketClientTest
{
    public class ClsSockteClient
    {
        #region field
        internal Socket sck;
        private string RmIp;
        private int port;
        public IDoWork iDoWork;
        public string errorMsg;
        #endregion

        public ClsSockteClient()
        {
            RmIp = "10.27.88.78";
            port = 6101;
            iDoWork = new DoWork1();
        }
        public void ConnectToServer()
        {
            try
            {
                sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sck.Connect(new IPEndPoint(IPAddress.Parse(RmIp), port));// RmIp和SPort分別為string和int型態, 前者為Server端的IP, 後者為Server端的Port
                Console.WriteLine("--Start Connection--");
                while (sck.Connected)
                {
                    iDoWork.Handle(this);
                }
                Console.WriteLine("--Close Application--");
            }
            catch (Exception ex)
            {
                this.errorMsg = ex.StackTrace;
                if (this.CloseConnect())
                {
                    Console.WriteLine("--Error and End Connection--");
                }
                else
                {
                    sck.Close();
                    sck = null;
                }
            }
        }

        public bool CloseConnect()
        {
            if (sck != null && sck.Connected)
            {
                try
                {
                    sck.Shutdown(SocketShutdown.Both);
                    sck.Close();
                    Console.WriteLine("--End Connection--");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:" + ex.Message);
                    return false;
                }
            }
            return false;
        }

    }
}
