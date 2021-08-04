using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using FTPClient.Core;
using FTPClient.WhyFTP.Exceptions;
using NLog;

namespace FTPClient.WhyFTP
{
    public class WhyFTPClient : IFTPClient
    {
        private string _host;
        private int _port;
        private FTPCredential _credential;
        private OperationSystemType _os = OperationSystemType.Linux;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Control Connection，控制连接。
        /// </summary>
        public FTPSocket CmdSocket { get; private set; }

        /// <summary>
        /// 是否已连接。
        /// </summary>
        public bool Connected { get; private set; }

        public WhyFTPClient()
        {
        }

        public void Init(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _credential = new FTPCredential()
            {
                Username = username,
                Password = password
            };
        }

        public void Connect()
        {
            // var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            CmdSocket = new FTPSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                CmdSocket.Connect(_host, _port);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            if (!CmdSocket.Connected)
            {
                _logger.Fatal("无法连接服务器！");
                return;
            }
            FTPResponse response = CmdSocket.ReadResponse();
            if (response.StatusCode == FTPStatusCode.SendUserCommand)
            {
                _logger.Debug("服务器已准备好，准备登录");
                Connected = true;
                Login();
            }
            else
            {
                _logger.Error("连接服务器失败");
                Connected = false;
            }
        }

        public void Disconnect()
        {
            CmdSocket.Disconnect(false);
            Connected = false;
        }

        public void ChangeDirectory(string path)
        {
            CmdSocket.Request("CWD", path);
        }

        public void CreateDirectory(string path)
        {
            CmdSocket.Request("MKD", path);
        }

        public void DeleteDirectory(string path)
        {
            CmdSocket.Request("DELE", path);
        }

        /// <summary>
        /// 登录到FTP服务器
        /// </summary>
        private void Login()
        {
            FTPResponse response;

            _logger.Info("正在输入用户名");
            response = CmdSocket.Request("USER", _credential.Username);
            if (response.StatusCode == FTPStatusCode.SendPasswordCommand)
            {
                _logger.Info("需要密码");
                _logger.Info("正在输入密码");
                response = CmdSocket.Request("PASS", Regex.Replace(_credential.Password, ".", "*"));
                switch (response.StatusCode)
                {
                    case FTPStatusCode.NotLoggedIn:
                        _logger.Error("登录失败！");
                        break;
                    case FTPStatusCode.LoggedInProceed:
                        _logger.Info("登录成功！");
                        break;
                }
            }

            response = CmdSocket.Request("SYST");
            if (response.Body.ToUpper().Contains("unix"))
                _os = OperationSystemType.Unix;
            else if (response.Body.ToUpper().Contains("linux"))
                _os = OperationSystemType.Linux;
            else if (response.Body.ToUpper().Contains("windows"))
                _os = OperationSystemType.Windows;
            _logger.Info($"FTP服务器操作系统：{_os}");
        }

        /// <summary>
        /// 列出文件。
        /// 
        /// </summary>
        /// <returns>文件信息</returns>
        /// <exception cref="FTPClientConnectionException"></exception>
        /// <exception cref="FTPClientUnsupportedConnectionException"></exception>
        public IList<string> ListFileLines(string path)
        {
            List<string> files = new List<string>(); ;
            var response = CmdSocket.Request("PASV");
            if (response.StatusCode == FTPStatusCode.EnteringPassive)
            {
                string[] addrAndPort = response.Body.Split("(")[1].Split(")")[0].Split(",");
                string host = addrAndPort[0] + "." + addrAndPort[1] + "." + addrAndPort[2] + "." + addrAndPort[3];
                int port = (int.Parse(addrAndPort[4]) << 8) | int.Parse(addrAndPort[5]);
                FTPSocket dataSocket = new FTPSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                dataSocket.Connect(host, port);
                if (!dataSocket.Connected)
                {
                    // throw new FTPClientConnectionException("连接到数据端口失败！");
                    _logger.Fatal("无法连接到数据端口！");
                    return files;
                }

                response = CmdSocket.Request("LIST", path);
                if (response.StatusCode != FTPStatusCode.OpeningData)
                {
                    // throw new FTPClientConnectionException("服务器未打开数据连接", response);
                    _logger.Error("服务器未打开数据连接！");
                    return files;
                }


                while (true)
                {
                    // TODO 处理文件信息
                    var line = dataSocket.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;
                    files.Add(line);
                    // _logger.Info(line);
                }

                response = CmdSocket.ReadResponse();
                if (response.StatusCode != FTPStatusCode.ClosingData)
                {
                    // throw new FTPClientConnectionException("服务器未能正常关闭数据连接", response);
                    _logger.Error("服务器未能正常关闭数据连接！");
                }

                dataSocket.Disconnect(false);

                return files;
            }
            _logger.Error("不支持的数据连接模式！");

            return files;

            // throw new FTPClientUnsupportedConnectionException("不支持的数据连接模式", response);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file">待删除的文件的文件名</param>
        /// <exception cref="FTPClientActionException"></exception>
        public void DeleteFile(string file)
        {
            var response = CmdSocket.Request("DELE", file);
            if (response.StatusCode == FTPStatusCode.ActionNotTakenFileUnavailable)
            {
                _logger.Error("无法删除文件！");
                // throw new FTPClientActionException("无法删除文件！", response);
            }
        }

        public void RenameFile(string filePath, string newFilePath)
        {
            var response = CmdSocket.Request("RNFR", filePath);
            if (response.StatusCode == FTPStatusCode.FileCommandPending)
            {
                response = CmdSocket.Request("RNTO", newFilePath);
                if (response.StatusCode == FTPStatusCode.FileActionOK)
                {
                    _logger.Info("成功重命名文件！");
                }
                else
                {
                    _logger.Error("重命名文件失败！");
                }
            }
        }

        public string GetCurrentDirectory()
        {
            var response = CmdSocket.Request("PWD");
            if (response.StatusCode != FTPStatusCode.PathnameCreated)
            {
                return "";
            }
            int a = response.Body.IndexOf("\"");
            int b = response.Body.IndexOf("\"", a + 1);
            return response.Body.Substring(a + 1, b - a - 1);
        }

        public FTPFile[] ListFiles(string path)
        {
            var files = ListFileLines(path);
            FTPFile[] ftpFiles = new FTPFile[files.Count];
            for (var i = 0; i < files.Count; i++)
            {
                ftpFiles[i] = FTPFile.FromFTPListLine(_os, files[i], path);
            }

            return ftpFiles;
        }


        // TODO 实现上传下载
        public void UploadFile(string localPath, string remotePath, IFTPClient.ReportProgress reportProgressFunction)
        {
            var random = new Random();
            for (int i = 0; i <= 100; i++)
            {
                if (random.Next(1000) >= 995)
                {
                    reportProgressFunction(i / 100f, true, false);
                    return;
                }

                reportProgressFunction(i / 100f, false, false);
                Thread.Sleep(100);
            }
            reportProgressFunction(1, false, true);
        }

        public void DownloadFile(string remotePath, string localPath, IFTPClient.ReportProgress reportProgressFunction)
        {
            UploadFile(localPath, remotePath, reportProgressFunction);
        }
    }
}