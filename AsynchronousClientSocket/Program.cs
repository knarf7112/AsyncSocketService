using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace AsynchronousClientSocket
{
    enum tt
    {
        s0 = 0x00,
        s1 = 0x01,
        s2 = 0x02,
        s3 = 0xFF
    }
    class Program
    {
        static byte[] dstAry;
        
        public static int Main(String[] args)
        {
            //AsyncClient.StartClient();
            //return 0;
            //int s = (int)tt.s3;
            //byte[] b1 = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            //byte[] b2 = new byte[10] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            //byte[] dstAry = new byte[b1.Length + b2.Length];
            //byte[,] ss = new byte[4, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
            //byte[][][] s2 = new byte[5][][];s2[0] = new byte[1][];s2[0][0] = new byte[1];
            
            //測試的Client
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sck.Connect("10.27.88.59", 6101);
            bool flag = false;
            bool state1 = true;
            bool state2 = !state1;
            int offset = 0;
            int maxOffset = 0;
            int size = 1024;
            byte[] receiveBytes;
            string sendCmd;
            byte[] sendBytes;
            string token = null;
            while (sck.Connected)
            {
                if (state1)
                {
                    //1.先送出要求訊息0001AAAA1000
                    sendCmd = Console.ReadLine().Replace("\n", "");
                    sendBytes = Encoding.GetEncoding("Big5").GetBytes(sendCmd);
                    sck.Send(sendBytes);

                    //2.取得訊息與檔案大小
                    receiveBytes = new byte[1025];
                    int receiveSize = sck.Receive(receiveBytes, receiveBytes.Length, SocketFlags.None);
                    string receiveStr = Encoding.GetEncoding("Big5").GetString(receiveBytes, 0, receiveSize);
                    Console.WriteLine(receiveStr);
                    if (!flag && receiveStr.Substring(0, 0) == "")//抓版本,目前沒有版本資訊
                    {
                        dstAry = new byte[int.Parse(receiveStr.Substring(0, 6))];//取得檔案Bytes大小並建立空byte陣列
                        maxOffset = Convert.ToInt32(Math.Floor((decimal)dstAry.Length / size));
                        token = receiveStr.Substring(6, 4);
                        flag = !flag;
                        state1 = !state1;
                    }
                }
                //state2: 傳送0001 + offset + size的command
                else
                {
                    //1.先送出要求訊息0001 + Token(4碼) + offset(4碼=>0000) + size
                    sendCmd = "0001" + token + offset.ToString("D4") + size;//Console.ReadLine().Replace("\n", "");
                    sendBytes = Encoding.GetEncoding("Big5").GetBytes(sendCmd);
                    Console.WriteLine(sendCmd);
                    sck.Send(sendBytes);
                    
                    //2.取得訊息與檔案大小
                    offset = Convert.ToInt32(sendCmd.Substring(8, 4));//端末編號(xxxx) + Token(xxxx) + offset(xxxx) + size
                    
                    receiveBytes = new byte[1026];
                    int receiveSize = sck.Receive(receiveBytes, receiveBytes.Length, 0);
                    byte tmpOffset1 = receiveBytes[receiveBytes.Length - 2];
                    byte tmpOffset2 = receiveBytes[receiveBytes.Length - 1];
                    int actualOffset = (tmpOffset1 * 256) + tmpOffset2;
                    //是否為最後一個package(可能為)
                    if (offset == maxOffset)
                    {
                        ArraySegment<byte> lastAry = new ArraySegment<byte>(receiveBytes,0,receiveSize - 2);

                        System.Buffer.BlockCopy(lastAry.ToArray(), 0, dstAry, (offset * (receiveBytes.Length - 2)), lastAry.Count);
                        using (FileStream fs = new FileStream(@"d:\temp\r6.jpg", FileMode.Create))
                        {
                            fs.Write(dstAry, 0, dstAry.Length);
                            fs.Flush();
                        }
                        sck.Shutdown(SocketShutdown.Both);
                        sck.Close();
                        sck.Dispose();
                        break;
                    }
                    else
                    {
                        try
                        {
                            if (offset == actualOffset)//若要求的offset==Server傳回的offset
                            {
                                System.Buffer.BlockCopy(receiveBytes, 0, dstAry, offset * (receiveBytes.Length - 2), receiveBytes.Length - 2);
                                offset++;
                            }
                        }
                        catch (Exception ex)
                        {
                            offset--;
                        }
                    }
                }
            }
            Console.ReadKey();
            return 0;
        }

        public static ManualResetEvent alldone = new ManualResetEvent(false);
        public static void CombineArray(byte[] srcAry,byte[] dstAry, int dstOffset,int srcOffset = 0)
        {
            try
            {
                System.Buffer.BlockCopy(srcAry, srcOffset, dstAry, (dstOffset * srcAry.Length), srcAry.Length);
            }
            catch (ArgumentException ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void ConnectCallBack(IAsyncResult ar)
        {
            var check = alldone.Set();
            Socket sck = (Socket)ar.AsyncState;
            sck.EndConnect(ar);
        }
        public static void Connect(string host,int port)
        {
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //sck.BeginConnect(host,port,);
            var check = alldone.Reset();
            Console.WriteLine("Establishing Connection to {0}",host);
            sck.BeginConnect(host, port, new AsyncCallback(ConnectCallBack), sck);
            alldone.WaitOne();
            Console.WriteLine("Connection established");
        }
    }
}
