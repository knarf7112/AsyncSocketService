using System;
using System.IO;
using System.Text;

namespace SocketClientTest
{
    //工作二:寫入檔案
    class DoWork2 :IDoWork
    {
        private byte[] receiveData;
        internal string totalFileSize;
        internal string token;
        private string filePath;
        private int segmentsize;
        private bool IsAuto;
        private string cmd;
        public DoWork2(DoWork1 dowork1)
        {
            this.totalFileSize = dowork1.totalFileSize;
            this.token = dowork1.token;
            this.segmentsize = 1024;
            this.filePath = @"d:\temp\r6.jpg";
            this.IsAuto = false;
        }

        public void Handle(ClsSockteClient client)
        {
            //檢查client端檔案並取得需要的Offset位置(-1表示檔案已傳輸完成)
            int sendOffset = GetNextFileOffset(filePath,Convert.ToInt32(this.totalFileSize),this.segmentsize);
            //輸入命令是否自動
            if (!IsAuto)
            {
                Console.WriteLine("工作二:cmd => auto(自動取檔) 或 0001+xxx(toekn)+xxxx(offset)+1024(size)");
                cmd = Console.ReadLine();
                if (cmd == "auto")
                {
                    IsAuto = true;
                }
            }
                //nextOffset==-1表示檔案已傳輸完成
            if (sendOffset != -1)
            {
                cmd = IsAuto ? "0001" + this.token + sendOffset.ToString("D4") + this.segmentsize : cmd;//是否自動取檔
                Console.WriteLine("送出命令: " + cmd);
                byte[] cmdBytes = Encoding.Default.GetBytes(cmd.Replace("\n", ""));//字串命令轉換 string=>byte[]
                int sendData = client.sck.Send(cmdBytes);//送出命令
                //送出檔案大小==成功傳送檔案大小
                if (sendData == cmdBytes.Length)
                {
                    this.receiveData = new byte[1026];
                    int receiveSize;
                    try
                    {
                        receiveSize = client.sck.Receive(receiveData, this.receiveData.Length, System.Net.Sockets.SocketFlags.None);

                        int receiveOffset = receiveData[receiveSize - 2] * 256 + receiveData[receiveSize - 1];//目前直接抓取已寫入檔案的大小來當下次offset所以offset沒用
                        if (sendOffset != receiveOffset)
                        {
                            Console.WriteLine("Transfer Error : 送出Offset非回傳Offset!");
                            return;
                        }
                        Array.Resize<byte>(ref receiveData, receiveSize - 2);
                        WriteFile(receiveData, this.filePath);
                        int localFileSize = File.ReadAllBytes(this.filePath).Length;
                        Console.WriteLine("寫入進度: " + localFileSize + " / " + this.totalFileSize);
                        if (localFileSize == Convert.ToInt32(this.totalFileSize))
                        {
                            client.CloseConnect();
                            Console.WriteLine("--傳輸完畢--");
                        }
                    }
                    catch
                    {
                        if (client.CloseConnect())
                            client.ConnectToServer();
                    }
                }
                else
                {
                    Console.WriteLine("成功送出大小!=預計送出大小: " + sendData + " / " + cmdBytes.Length);
                }
            }
            else
            {
                Console.WriteLine("檔案已傳輸完畢: Total Size => " + this.totalFileSize + "bytes");
                client.CloseConnect();
            }
            
        }
        /// <summary>
        /// 檢查client端檔案並回傳下個Offset位置
        /// </summary>
        /// <param name="filePath">檔案完整存放路徑</param>
        /// <param name="totalSize">要比較的原始檔案完整大小</param>
        /// <param name="segmentsize">檔案封包大小</param>
        /// <returns>下個Offset位置(-1表示檔案size與totalSize一樣)</returns>
        protected int GetNextFileOffset(string filePath, int totalSize, int segmentsize)
        {
            if (File.Exists(filePath))
            {
                byte[] fileSize = File.ReadAllBytes(filePath);
                if (fileSize.Length == totalSize)
                {
                    return -1;
                }
                else
                {
                    return Convert.ToInt32(Math.Floor((decimal)fileSize.Length / segmentsize));
                }
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 資料寫入檔案
        /// </summary>
        /// <param name="data">要寫入的資料陣列</param>
        /// <param name="filePath">檔案完整路徑</param>
        /// <param name="offset">寫入的Offset位置</param>
        /// <param name="fileMode">檔案模式預設為Append</param>
        protected void WriteFile(byte[] data,string filePath, int offset = 0,FileMode fileMode = FileMode.Append)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, fileMode, FileAccess.Write))
                {
                    fs.Write(data, offset, data.Length);
                    fs.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("寫入檔案錯誤: " + ex.Message);
            }
        }
    }
}
