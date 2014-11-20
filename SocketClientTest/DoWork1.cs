using System;
using System.Net.Sockets;
using System.Text;

namespace SocketClientTest
{
    //送出需求命令並驗證,成功則轉換至工作二
    class DoWork1 : IDoWork
    {
        protected byte[] receiveData;
        private int count;
        internal string totalFileSize;
        internal string token;

        public DoWork1()
        {
            count = 0;
        }
        public void Handle(ClsSockteClient client)
        {
            Console.WriteLine("工作一: 輸入0001(端末)+AAAA(pinCode)+1001(firmwareVers)做驗證");
            string cmd = Console.ReadLine();
            if (this.token != null && cmd == this.token)
            {
                client.iDoWork = new DoWork2(this);
                client.ConnectToServer();
                return;
            }
            byte[] sendData = Encoding.Default.GetBytes(cmd.Replace("\n",""));
            try
            {
                client.sck.Send(sendData);
                Console.WriteLine("送出的命令:" + cmd);
                receiveData = new byte[1024];
                int receivebytes = client.sck.Receive(receiveData, receiveData.Length, SocketFlags.None);
                if (receivebytes > 0)
                {
                    string receiveCmd = Encoding.Default.GetString(receiveData, 0, receivebytes);
                    if (IsLastestVerssion(receiveCmd) && CheckCmdLength(receiveCmd))
                    {
                        this.totalFileSize = receiveCmd.Substring(0, 6);//4, 6);//0001+269610
                        this.token = receiveCmd.Substring(6, 4);//10, 4);
                        Console.WriteLine("檔案大小:" + Convert.ToInt32(this.totalFileSize) + " \n驗證碼:" + this.token);
                        if (client.CloseConnect())
                        {
                            client.iDoWork = new DoWork2(this);
                            client.ConnectToServer();//reconnection
                        }
                        else
                        {
                            Console.WriteLine(" Close Connection failed!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("驗證錯誤:請重新連線!");
                        client.CloseConnect();
                    }
                }
                else
                {
                    count++;
                }
                if (count >= 3)
                {
                    Console.WriteLine("傳0bytes次數超過3次,關閉連線");
                    client.CloseConnect();
                }
            }
            catch
            {
                if (client.CloseConnect())
                    client.ConnectToServer();
            }
        }
        //檢查命令長度
        private bool CheckCmdLength(string receiveCmd)
        {
            return true;
        }

        //檢查版本
        private bool IsLastestVerssion(string receiveCmd)
        {
            return true;
        }
    }
}
