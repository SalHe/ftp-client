using System.Collections.ObjectModel;
using System.Reactive;
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
        }
    }
}
