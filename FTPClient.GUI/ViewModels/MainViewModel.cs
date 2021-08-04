using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DynamicData;
using FTPClient.Core;
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
            set
            {
                this.RaiseAndSetIfChanged(ref _remoteDirectory, value);
                this.RaisePropertyChanged(nameof(TargetRemoteFilePath));
            }
        }

        public FileModel SelectedLocalFile
        {
            get => _selectedLocalFile;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedLocalFile, value);
                this.RaisePropertyChanged(nameof(TargetRemoteFilePath));
                _uploadSelectedFileCommandCanExec.OnNext(Connected && SelectedLocalFile != null && File.Exists(SelectedLocalFile.FilePath));
            }
        }

        public RemoteFileModel SelectedRemoteFile { get; set; }
        public string TargetRemoteFilePath => SelectedLocalFile == null ? ""
            : (RemoteDirectory.EndsWith("/") ? RemoteDirectory + SelectedLocalFile.FileName : RemoteDirectory + "/" + SelectedLocalFile.FileName);

        public ObservableCollection<FileModel> LocalFiles { get; }
        public ObservableCollection<RemoteFileModel> RemoteFiles { get; }
        public ObservableCollection<TransferFileModel> TransferTasks { get; }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> ConnectFtpServerCommand { get; }
        public ReactiveCommand<Unit, Unit> DisconnectFromFtpServeCommand { get; }
        public ReactiveCommand<string, Unit> ChangeLocalDirectoryCommand { get; }
        public ReactiveCommand<RemoteFileModel, Unit> ChangeRemoteDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> UploadSelectedFileCommand { get; }

        private Subject<bool> _uploadSelectedFileCommandCanExec;

        #endregion

        private IFTPClient _ftpClient;
        private FileModel _selectedLocalFile;

        public MainViewModel()
        {
            _ftpClient = new FakeFTPClient();

            LocalFiles = new();
            RemoteFiles = new();
            TransferTasks = new();

            ConnectFtpServerCommand = ReactiveCommand.Create(() =>
           {
               _ftpClient.Init(Host, Port, Username, Password);
               _ftpClient.Connect();
               Connected = _ftpClient.Connected;
               ChangeRemoteDirectoryToRoot();
           });

            DisconnectFromFtpServeCommand = ReactiveCommand.Create(() =>
           {
               _ftpClient.Disconnect();
               Connected = _ftpClient.Connected;
               if (!Connected) RemoteFiles.Clear();
           });

            ChangeLocalDirectoryCommand = ReactiveCommand.CreateFromTask(new Func<string, Task<Unit>>(ChangeLocalDirectory));
            ChangeRemoteDirectoryCommand = ReactiveCommand.CreateFromTask(new Func<RemoteFileModel, Task<Unit>>(ChangeRemoteDirectory));

            // TODO 不知道这个canExecute有没有简单一点的写法
            _uploadSelectedFileCommandCanExec = new Subject<bool>();
            UploadSelectedFileCommand = ReactiveCommand.Create(
                () => UploadFile(SelectedLocalFile, TargetRemoteFilePath),
                _uploadSelectedFileCommandCanExec
            );
        }

        private void UploadFile(FileModel localFile, string targetPath)
        {
            TransferTasks.Add(new TransferFileModel()
            {
                FilePath = localFile.FilePath,
                TransferDirection = TransferDirection.Upload,
                RemoteFilePath = targetPath,
                Size = localFile.Size,
                Time = localFile.Time,
                Progress = 0,
                TransferStatus = TransferStatus.Ready
            });
        }

        public void ChangeRemoteDirectoryToRoot()
        {
            ChangeRemoteDirectory("/");
        }

        public async Task<Unit> ChangeLocalDirectory(string dir)
        {
            var files = new List<FileModel>();
            if (string.IsNullOrEmpty(dir) || Regex.IsMatch(dir, @"^\w:\\+\.\.$"))
            {
                dir = "";
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

        public async Task<Unit> ChangeRemoteDirectory(RemoteFileModel dir)
        {
            if (!dir.Grants.StartsWith("d")) return Unit.Default;
            var path = dir.FilePath;
            ChangeRemoteDirectory(path);
            return Unit.Default;
        }

        private void ChangeRemoteDirectory(string path)
        {
            if (!Connected)
            {
                UIUtils.RunOnUIThread(() => RemoteFiles.Clear());
                return;
            }
            _ftpClient.ChangeDirectory(path);
            List<RemoteFileModel> remoteFiles = new();

            // 添加一个特殊目录，用于返回父级目录
            if (!Regex.IsMatch(path, @"^/+$"))
                remoteFiles.Add(new RemoteFileModel()
                {
                    FilePath = path + "/..",
                    Grants = "d------"
                });

            foreach (FTPFile ftpFile in _ftpClient.ListFiles(path))
            {
                remoteFiles.Add(new RemoteFileModel()
                {
                    FilePath = ftpFile.FilePath,
                    Size = ftpFile.Size,
                    Grants = ftpFile.Grants,
                    Owner = ftpFile.Owner + "/" + ftpFile.OwnerGroup,
                    Time = DateTime.Parse(ftpFile.Time)
                });
            }

            UIUtils.RunOnUIThread(() =>
            {
                RemoteFiles.Clear();
                RemoteFiles.AddRange(remoteFiles);
            });
            RemoteDirectory = _ftpClient.GetCurrentDirectory();
        }
    }
}
