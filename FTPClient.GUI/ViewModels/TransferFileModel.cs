using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace FTPClient.GUI.ViewModels
{
    public class TransferFileModel : FileModel
    {
        private TransferDirection _transferDirection;
        private string _remoteFilePath;
        private int _progress;
        private TransferStatus _transferStatus;

        public TransferDirection TransferDirection
        {
            get => _transferDirection;
            set => this.RaiseAndSetIfChanged(ref _transferDirection, value);
        }
        public string RemoteFilePath
        {
            get => _remoteFilePath;
            set => this.RaiseAndSetIfChanged(ref _remoteFilePath, value);
        }
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public TransferStatus TransferStatus
        {
            get => _transferStatus;
            set => this.RaiseAndSetIfChanged(ref _transferStatus, value);
        }
    }

    public enum TransferDirection
    {
        Upload, Download
    }

    public enum TransferStatus
    {
        Ready, Transferring, Done, Error
    }
}
