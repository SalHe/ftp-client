using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace FTPClient.Core
{
    public class FakeFTPClient : IFTPClient
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _connected;
        private string _host;
        private int _port;
        private string _username;
        private string _password;
        private string _currentDir;

        public bool Connected => _connected;
        public void Init(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }

        public void Connect()
        {
            Thread.Sleep(2900);
            _connected = "123456".Equals(_password);
        }

        public void Disconnect()
        {
            Thread.Sleep(1500);
            _connected = false;
        }

        public void ChangeDirectory(string path)
        {
            var dirs = Regex.Split(path, @"/+");
            int p = -1;
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Equals(".."))
                {
                    p = i;
                    break;
                }
            }

            if (p >= 0)
            {
                int returns = dirs.Length - p;
                path = string.Join("/", dirs, 0, p - returns);
            }
            if (path.Equals("")) path = "/";
            if ("/".Equals(path) || Regex.IsMatch(path, @"folder\d+$"))
                _currentDir = path;
            _logger.Debug($"切换目录：{_currentDir}");
        }

        public void CreateDirectory(string path)
        {
            _logger.Debug($"创建文件夹：{path}");
        }

        public void DeleteDirectory(string path)
        {
            _logger.Debug($"删除文件夹：{path}");
        }

        public void DeleteFile(string file)
        {
            _logger.Debug($"删除文件：{file}");
        }

        public void RenameFile(string filePath, string newFilePath)
        {
            _logger.Debug($"重命名：{filePath}");
            _logger.Debug($"新文件名：{newFilePath}");
        }

        public string GetCurrentDirectory()
        {
            if (!Connected)
            {
                return "";
            }
            return _currentDir;
        }

        public FTPFile[] ListFiles(string path)
        {
            if (!Connected) return Array.Empty<FTPFile>();
            List<FTPFile> files = new();
            var random = new Random();
            int count = random.Next(10) + 1;
            string splash = _currentDir.EndsWith("/") ? "" : "/";
            for (int i = 0; i < count; i++)
            {
                files.Add(new FTPFile()
                {
                    FilePath = _currentDir + splash + "folder" + i,
                    Grants = "drw-------",
                    Owner = "owner",
                    OwnerGroup = "group",
                    Size = -1,
                    Time = DateTime.Now.ToString()
                });
            }

            count = random.Next(30);
            for (int i = 0; i < count; i++)
            {
                files.Add(new FTPFile()
                {
                    FilePath = _currentDir + splash + "file" + i + ".txt",
                    Grants = "-rw-------",
                    Owner = "owner",
                    OwnerGroup = "group",
                    Size = random.Next(),
                    Time = DateTime.Now.ToString()
                });
            }
            _logger.Debug("文件列表：...");
            // files.ForEach(x => _logger.Debug(x));
            return files.ToArray();
        }

        public void UploadFile(string localPath, string remotePath, IFTPClient.ReportProgress reportProgressFunction)
        {
            var random = new Random();
            for (int i = 0; i <= 100; i++)
            {
                if (random.Next(10) > 7)
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
