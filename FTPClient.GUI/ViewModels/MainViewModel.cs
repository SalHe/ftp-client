using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DynamicData;
using ReactiveUI;

namespace FTPClient.GUI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        #region Properties

        private string _host = "127.0.0.1";
        private int _port = 21;
        private string _username = "admin";
        private string _password = string.Empty;
        private bool _connected;
        private string _localDirectory;
        private string _remoteDirectory;

        public string Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref _host, value);
        }
        public int Port
        {
            get => _port;
            set => this.RaiseAndSetIfChanged(ref _port, value);
        }
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        public bool Connected
        {
            get => _connected;
            set
            {
                this.RaiseAndSetIfChanged(ref _connected, value);
                this.RaisePropertyChanged(nameof(NotConnected));
            }
        }
        public bool NotConnected => !Connected;
        public string LocalDirectory
        {
            get => _localDirectory;
            set => this.RaiseAndSetIfChanged(ref _localDirectory, value);
        }
        public string RemoteDirectory
        {
            get => _remoteDirectory;
            set => this.RaiseAndSetIfChanged(ref _remoteDirectory, value);
        }
        public ObservableCollection<FileModel> LocalFiles { get; }
        public ObservableCollection<RemoteFileModel> RemoteFiles { get; }
        public ObservableCollection<TransferFileModel> TransferTasks { get; }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> ConnectFtpServerCommand { get; }
        public ReactiveCommand<Unit, Unit> DisconnectFromFtpServeCommand { get; }
        public ReactiveCommand<string, Unit> ChangeLocalDirectoryCommand { get; }
        public ReactiveCommand<string, Unit> ChangeRemoteDirectoryCommand { get; }

        #endregion

        public MainViewModel()
        {
            LocalFiles = new();
            RemoteFiles = new();
            TransferTasks = new();

            ConnectFtpServerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO 实现连接到服务器
                Connected = true;
            });

            DisconnectFromFtpServeCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // TODO 实现断开连接
                Connected = false;
            });

            ChangeLocalDirectoryCommand = ReactiveCommand.CreateFromTask(new Func<string, Task<Unit>>(ChangeLocalDirectory));
            ChangeRemoteDirectoryCommand = ReactiveCommand.CreateFromTask(new Func<string, Task<Unit>>(ChangeRemoteDirectory));
        }

        public async Task<Unit> ChangeLocalDirectory(string dir)
        {
            var files = new List<FileModel>();
            if (string.IsNullOrEmpty(dir) || Regex.IsMatch(dir, @"^\w:\\+\.\.$"))
            {
                // 当路径为空时或者从驱动器根目录回退时展示驱动器
                foreach (string drive in Directory.GetLogicalDrives())
                {
                    files.Add(new FileModel()
                    {
                        FilePath = drive,
                        Size = -1,
                        Time = Directory.GetLastWriteTime(drive)
                    });
                }
            }
            else
            {
                dir = Path.GetFullPath(dir);    // 转为绝对路径
                if (!Directory.Exists(dir))
                    return Unit.Default;
                files.Add(new FileModel()
                {
                    FilePath = Path.Join(dir, ".."),
                    Size = -1,
                    Time = DateTime.MaxValue
                });
                foreach (string fullPath in Directory.EnumerateDirectories(dir))
                {
                    files.Add(new FileModel()
                    {
                        FilePath = fullPath,
                        Size = -1,
                        Time = Directory.GetLastWriteTime(fullPath)
                    });
                }

                foreach (string fullPath in Directory.EnumerateFiles(dir))
                {
                    files.Add(new FileModel()
                    {
                        FilePath = fullPath,
                        Size = new FileInfo(fullPath).Length,
                        Time = File.GetLastWriteTime(fullPath)
                    });
                }
            }

            LocalDirectory = dir;
            UIUtils.RunOnUIThread(() =>
            {
                LocalFiles.Clear();
                LocalFiles.AddRange(files);
            });
            return Unit.Default;
        }

        public async Task<Unit> ChangeRemoteDirectory(string dir)
        {
            // TODO 获取远程的文件列表
            RemoteDirectory = dir;
            UIUtils.RunOnUIThread(() =>
            {
                RemoteFiles.Clear();
                RemoteFiles.Add(new RemoteFileModel()
                {
                    FilePath = @"/home/1.txt",
                    Size = 5,
                    Time = DateTime.Now,
                    Grants = "-rw-------",
                    Owner = "ftp/ftp"
                });
                RemoteFiles.Add(new RemoteFileModel()
                {
                    FilePath = @"/home/2.txt",
                    Size = 5,
                    Time = DateTime.Now,
                    Grants = "-rw-------",
                    Owner = "ftp/ftp"
                });
            });
            return Unit.Default;
        }
    }
}
