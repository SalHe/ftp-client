using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace FTPClient.GUI
{
    public class MainViewModel : ReactiveObject
    {
        private string _host = "127.0.0.1";
        private int _port = 21;
        private string _username = "admin";
        private string _password = string.Empty;
        private bool _connected;

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
            set => this.RaiseAndSetIfChanged(ref _connected, value);
        }

        public bool NotConnected => !Connected;
    }
}
