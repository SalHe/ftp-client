using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FTPClient.Core.XyxFTP
{
    public class XyxFTPClient : IFTPClient
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        private string serverAddress;

        /// <summary>
        /// 用户名
        /// </summary>
        private string username;

        /// <summary>
        /// 用户密码
        /// </summary>
        private string password;

        /// <summary>
        /// 每执行一个方法产生的响应存至此处
        /// </summary>
        private string cmdResponse_1;

        /// <summary>
        /// 每执行某操作一次方法产生的响应如不够存就存至此处
        /// </summary>
        private string cmdResponse_2;

        /// <summary>
        /// 0 - 操作执行失败
        /// 1 - 操作执行成功
        /// </summary>
        private int debugMessage;

        /// <summary>
        /// 数据端口接受的响应，用于获取DIR时
        /// </summary>
        private string dataResponse;

        /// <summary>
        /// 命令socket
        /// </summary>
        private Socket cmdClient;

        /// <summary>
        /// 数据socket
        /// </summary>
        private Socket dataClient;

        /// <summary>
        /// 通用缓冲区大小,一般（其实是几乎）接受命令socket的响应的缓冲区就是这么大
        /// </summary>
        private readonly int bufSize = 1024;

        /// <summary>
        /// 多次传输是每此传输的缓冲区大小
        /// </summary>
        private readonly int tempBufSize = 10;

        /// <summary>
        /// 此次操作是否有两个响应
        /// true 有两个响应各存于 cmdResponse_1、cmdResponse_2
        /// false - 只有一个响应存于 cmdResponse_1
        /// </summary>
        private bool twoResponse = false;

        private OperationSystemType _os;

        public string CmdResponse_1
        {
            get
            {
                if (cmdResponse_1 == null)
                    return "";
                return cmdResponse_1;
            }
        }
        public string CmdResponse_2
        {
            get
            {
                if (cmdResponse_2 == null)
                    return "";
                return cmdResponse_2;
            }
        }
        public string ServerAddress
        {
            set
            {
                if (value != null)
                    serverAddress = value;
            }
            get
            {
                if (serverAddress == null)
                    return "";
                return serverAddress;
            }
        }
        public string Username
        {
            set
            {
                if (value != null)
                    username = value;
            }
            get
            {
                if (username == null)
                    return "";
                return username;
            }
        }
        public string Password
        {
            set
            {
                if (value != null)
                    password = value;
            }
            get
            {
                if (password == null)
                    return "";
                return password;
            }
        }
        public int DebugMessage
        {
            get
            {
                return debugMessage;
            }
        }
        public bool TwoResponse
        {
            get
            {
                return twoResponse;
            }

        }

        bool IFTPClient.Connected => IsConnected();


        private bool IsConnected()
        {
            if (cmdClient == null)
                return false;
            return cmdClient.Connected;
        }


        /// <summary>
        /// 构造方法
        /// </summary>
        public XyxFTPClient()
        {
            cmdClient = null;
            dataClient = null;
            serverAddress = "";
            // 1表示执行成功
            // 0表示执行失败
            debugMessage = 1;
        }

        /// <summary>
        /// 清空响应
        /// </summary>
        private void ResetResponse()
        {
            twoResponse = false;
            cmdResponse_1 = "";
            cmdResponse_2 = "";
            dataResponse = "";
        }

        /// <summary>
        /// 发送命令
        /// mode:
        /// 0 - 命令套接字
        /// 1 - 数据套接字
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mode"></param>
        private void SendCmd(string cmd, int mode)
        {
            byte[] buffer = new byte[bufSize];
            buffer = System.Text.Encoding.Default.GetBytes(cmd);
            try
            {
                switch (mode)
                {
                    case 0:
                        if (cmdClient == null)
                            return;
                        cmdClient.Send(buffer);
                        break;
                    case 1:
                        if (dataClient == null)
                            return;
                        dataClient.Send(buffer);
                        break;
                }
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 接受响应
        /// mode:
        /// 0 - 命令套接字
        /// 1 - 数据套接字
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private int receive(int mode)
        {
            int length = 0;
            byte[] buffer = new byte[bufSize];
            try
            {
                switch (mode)
                {
                    case 0:
                        if (cmdClient == null)
                            return 0;
                        length = cmdClient.Receive(buffer);
                        cmdResponse_1 = "";
                        cmdResponse_1 = System.Text.Encoding.ASCII.GetString(buffer);
                        break;
                    case 1:
                        if (dataClient == null)
                            return 0;
                        length = dataClient.Receive(buffer);
                        dataResponse = "";
                        dataResponse = System.Text.Encoding.ASCII.GetString(buffer);
                        break;
                    case 3:
                        if (cmdClient == null)
                            return 0;
                        length = cmdClient.Receive(buffer);
                        cmdResponse_2 = "";
                        cmdResponse_2 = System.Text.Encoding.ASCII.GetString(buffer);
                        break;
                }
                debugMessage = 0;
                return length;
            }
            catch (Exception)
            {
                debugMessage = 1;
                return 0;
            }

        }

        /// <summary>
        /// 供命令行使用
        /// </summary>
        /// <param name="cmd"></param>
        public void Cmd(string cmd)
        {
            ResetResponse();
            try
            {
                cmdResponse_1 = "";
                if (cmdClient == null || cmdClient.Connected == false)
                    CmdConnect();
                SendCmd(cmd + "\r\n", 0);
                receive(0);
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 设置账户信息
        /// </summary>
        /// <param name="u"></param>
        /// <param name="p"></param>
        public void SetAccount(string u, string p)
        {
            this.username = u;
            this.password = p;
        }

        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="domain"></param>
        public void SetServer(string domain)
        {
            // 这里由于本机调试，故没有使用dns
            // serverAddress = DNS.GetIP(domain);
            serverAddress = domain;
        }

        /// <summary>
        /// 建立命令连接
        /// </summary>
        public void CmdConnect()
        {
            ResetResponse();
            byte[] buffer = new byte[bufSize];
            cmdClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverAddress), 21);
            try
            {
                int i;
                cmdClient.Connect(serverAddress, 21);
                cmdClient.Receive(buffer);
                SendCmd("USER " + username + "\r\n", 0);
                receive(0);
                SendCmd("PASS " + password + "\r\n", 0);
                receive(3);
                if (cmdResponse_2.Substring(0, 3) != "230")
                {
                    cmdClient.Close();
                }
                twoResponse = true;
                i = cmdResponse_1.IndexOf("\r\n");
                cmdResponse_1 = cmdResponse_1.Substring(0, i);
                
                SendCmd("SYST\r\n", 0);
                receive(0);
                if (cmdResponse_1.ToUpper().Contains("LINUX"))
                    _os = OperationSystemType.Linux;
                else if (cmdResponse_1.ToUpper().Contains("UNIX"))
                    _os = OperationSystemType.Unix;
                else if (cmdResponse_1.ToUpper().Contains("WINDOWS"))
                    _os = OperationSystemType.Windows;
                debugMessage = 0;
            }
            catch (Exception)
            {
                debugMessage = 1;
            }
        }

        /// <summary>
        /// 建立数据连接
        /// </summary>
        public void DataConnect()
        {
            ResetResponse();
            try
            {
                // 命令socket未建立连接直接返回
                if (cmdClient == null)
                    return;
                // 变量声明
                string response;
                string code;
                byte[] buffer_0 = new byte[bufSize];
                byte[] buffer_1 = new byte[bufSize];
                string[] parts;
                string address;
                int port;

                // 请求被动模式
                buffer_0 = System.Text.Encoding.Default.GetBytes("PASV\r\n");
                cmdClient.Send(buffer_0);
                cmdClient.Receive(buffer_1);

                // PASV端口获取
                response = System.Text.Encoding.Default.GetString(buffer_1);
                int i = response.IndexOf('(');
                int j = response.IndexOf(')');
                code = response.Substring(0, 3);
                response = response.Substring(i + 1, j - i - 1);
                parts = response.Split(',');
                address = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
                port = Convert.ToInt32(parts[4]) * 256 + Convert.ToInt32(parts[5]);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(address), port);
                dataClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                dataClient.Connect(ep);
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 获取指定目录信息
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public string[] Dir(string dir)
        {
            ResetResponse();
            try
            {
                // 确保连接
                if (cmdClient == null || cmdClient.Connected == false)
                    CmdConnect();
                if (dataClient == null || dataClient.Connected == false)
                    DataConnect();

                // 请求目录
                SendCmd("LIST\r\n", 0);
                receive(0);
                receive(3);
                twoResponse = true;
                receive(1);

                // 关闭数据链接
                if (dataClient != null)
                    dataClient.Close();
                debugMessage = 1;
                return dataResponse.Split("\r\n");
            }
            catch (Exception)
            {
                debugMessage = 1;
                return new string[0];
            }
        }

        /// <summary>
        /// 切换目录
        /// </summary>
        /// <param name="dir"></param>
        public void Cwd(string dir)
        {
            ResetResponse();
            try
            {
                if (cmdClient == null || cmdClient.Connected == false)
                    CmdConnect();
                SendCmd("CWD " + dir + "\r\n", 0);
                receive(0);
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 获取当前目录
        /// </summary>
        /// <returns></returns>
        public string Pwd()
        {
            ResetResponse();
            try
            {
                string[] temp;
                if (cmdClient == null || cmdClient.Connected == false)
                    CmdConnect();
                SendCmd("PWD\r\n", 0);
                receive(0);
                temp = cmdResponse_1.Split("\"");
                debugMessage = 1;
                return temp[1];
            }
            catch (Exception)
            {
                debugMessage = 0;
                return "";
            }
        }

        /// <summary>
        /// 获取远程主机指定文件大小
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int Size(string fileName)
        {
            ResetResponse();
            try
            {
                SendCmd("SIZE " + fileName + "\r\n", 0);
                receive(0);
                int i = cmdResponse_1.IndexOf("213");
                int j = cmdResponse_1.IndexOf("\r\n");
                debugMessage = 1;
                if (cmdResponse_1.Substring(0, 3) == "530" || cmdResponse_1.Substring(0, 3) == "550")
                {
                    return 0;
                }
                return Convert.ToInt32(cmdResponse_1.Substring(i + 4, j - i - 4));
            }
            catch (Exception)
            {
                debugMessage = 0;
                return -1;
            }
        }

        /// <summary>
        /// 创建远程主机目录
        /// </summary>
        /// <param name="dirName"></param>
        public void Mkd(string dirName)
        {
            ResetResponse();
            try
            {
                if (cmdClient == null || cmdClient.Connected == false)
                    CmdConnect();
                byte[] buffer = new byte[bufSize];
                string cmd = "MKD " + dirName + "\r\n";
                buffer = System.Text.Encoding.Default.GetBytes(cmd);
                cmdClient.Send(buffer);
                receive(0);
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 下载、支持断点续传
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="remotePath"></param>
        /// <param name="reportProgressFunction"></param>
        public void NewDownload(string localPath, string remotePath, IFTPClient.ReportProgress reportProgressFunction)
        {
            try
            {

                int i;
                // 远程文件信息处理
                string[] parts = remotePath.Split("\\");
                string fileName = parts[parts.Length - 1];
                long fileSize = Size(fileName);

                // 找不到文件
                if (cmdResponse_1.Substring(0, 3) == "550")
                {
                    debugMessage = 0;
                    return;
                }

                localPath = localPath;
                // 本地文件创建
                FileStream file = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write);
                file.Seek(0, SeekOrigin.End);
                // 文件已下载大小,从后往前数
                long finishSize = file.Length;

                // 文件剩余大小（等价文件指针）
                long restSize = fileSize - finishSize;

                // 下载
                byte[] buffer = new byte[restSize];
                DataConnect();
                SendCmd("REST " + finishSize.ToString() + "\r\n", 0);
                receive(0);
                SendCmd("RETR " + remotePath + "\r\n", 0);
                receive(0);
                i = dataClient.Receive(buffer);
                do
                {
                    file.Write(buffer, 0, i);
                    finishSize += i;
                    reportProgressFunction((finishSize) / fileSize, false, false);
                    i = dataClient.Receive(buffer);
                    file.Flush();
                }
                while (i > 0);

                dataClient.Receive(buffer);
                dataClient.Close();
                reportProgressFunction(1, false, true);
                debugMessage = 1;
                if (dataClient == null)
                    return;
                dataClient.Close();
            }
            catch (Exception)
            {
                if (dataClient != null)
                    dataClient.Close();
                reportProgressFunction(0, true, false);
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 上传、支持断点续传
        /// </summary>
        /// <param name="filePath"></param>
        public void newUpoad(string localPath, string remotePath, IFTPClient.ReportProgress reportProgressFunction)
        {
            ResetResponse();
            try
            {
                // 剩余文件读取
                string[] parts;
                parts = localPath.Split("\\");
                FileStream input = new FileStream(localPath, FileMode.Open);
                string originPath = GetCurrentDirectory();
                ChangeDirectory(remotePath);
                long finishSize = Size(parts[parts.Length - 1]);
                long fileSize = input.Length;
                while (finishSize < fileSize)
                {
                    byte[] buffer;
                    long restSize = fileSize - finishSize;
                    if (tempBufSize > restSize)
                    {
                        buffer = new byte[restSize];
                    }
                    else
                    {
                        buffer = new byte[tempBufSize];
                    }
                    input.Seek(finishSize, SeekOrigin.Begin);
                    input.Read(buffer);
                    DataConnect();
                    //知会服务器追加文件名
                    if (finishSize > 0)
                    {
                        SendCmd("APPE " + parts[parts.Length - 1] + "\r\n", 0);
                        receive(0);
                    }
                    else
                    {
                        SendCmd("STOR " + parts[parts.Length - 1] + "\r\n", 0);
                        receive(0);
                    }
                    dataClient.Send(buffer);
                    dataClient.Close();
                    receive(0);
                    //receive(0);
                    finishSize += tempBufSize;
                    reportProgressFunction(finishSize / fileSize, false, false);
                }
                ChangeDirectory(originPath);
                twoResponse = true;
                input.Close();
                debugMessage = 1;
                reportProgressFunction(1, false, true);
            }
            catch (Exception)
            {
                reportProgressFunction(0, true, false);
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 下线
        /// </summary>
        public void Quit()
        {
            ResetResponse();
            try
            {
                SendCmd("QUIT\r\n", 0);
                receive(0);
                cmdClient.Close();
                if (dataClient != null)
                    dataClient.Close();
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteFile(string file)
        {
            ResetResponse();
            try
            {
                SendCmd("DELE " + file + "\r\n", 0);
                receive(0);
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 1;
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteDir(string dir)
        {
            ResetResponse();
            try
            {
                cmdResponse_1 = "";
                SendCmd("RMD " + dir + "\r\n", 0);
                receive(0);
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public void Rename(string oldPath, string newPath)
        {
            ResetResponse();
            try
            {
                SendCmd("RNFR " + oldPath + "\r\n", 0);
                receive(0);
                SendCmd("RNTO " + newPath + "\r\n", 0);
                receive(3);
                twoResponse = true;
                debugMessage = 1;
            }
            catch (Exception)
            {
                debugMessage = 0;
            }
        }

        /// <summary>
        /// 传路径，得名字
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetObjectName(string path)
        {
            string[] parts = path.Split("\\");
            return parts[parts.Length - 1];
        }

        #region
        public void Disconnect()
        {
            Quit();
        }

        public void ChangeDirectory(string path)
        {
            Cwd(path);
        }

        /// <summary>
        /// 在当前远程目录下创建
        /// </summary>
        /// <param name="path"></param>
        public void CreateDirectory(string path)
        {
            Mkd(path);
        }

        /// <summary>
        /// path为输入文件
        /// </summary>
        /// <param name="path"></param>
        public void DeleteDirectory(string path)
        {
            DeleteDir(path);
        }

        public string GetCurrentDirectory()
        {
            return Pwd();
        }

        public FTPFile[] ListFiles(string path)
        {
            string[] filelist = Dir(path);
            FTPFile[] file = new FTPFile[filelist.Length - 1];
            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "yyy MM hh:mm";
            for (int i = 0; i < filelist.Length - 1; i++)
            {
                file[i] = FTPFile.FromFTPListLine(_os, filelist[i], path);
                // file[i] = new FTPFile();
                // file[i].FilePath = GetCurrentDirectory() + "/" + filelist[i].Substring(49);
                // file[i].Grants = filelist[i].Substring(0, 10);
                // file[i].Time = Convert.ToDateTime(filelist[i].Substring(36, 12), dtFormat);
            }
            return file;
        }

        public void UploadFile(string localPath, string remotePath, IFTPClient.ReportProgress reportProgressFunction)
        {
            newUpoad(localPath, remotePath, reportProgressFunction);
        }

        public void DownloadFile(string remotePath, string localPath, IFTPClient.ReportProgress reportProgressFunction)
        {
            NewDownload(localPath, remotePath, reportProgressFunction);
        }

        public void Init(string host, int port, string username, string password)
        {
            SetAccount(username, password);
            SetServer(host);
        }

        public void Connect()
        {
            CmdConnect();
        }

        public void RenameFile(string filePath, string newFilePath)
        {
            Rename(filePath, newFilePath);
        }
        #endregion
    }
}
