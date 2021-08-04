using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ReactiveUI;

namespace FTPClient.GUI.ViewModels
{
    public class FileModel : ReactiveObject
    {
        private long _size;
        private DateTime _time;
        private string _filePath;
        private BitmapSource _fileIcon;


        public string FilePath
        {
            get => _filePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _filePath, value);
                this.RaisePropertyChanged(nameof(FileName));

                UIUtils.RunOnUIThread(() =>
                {
                    // TODO 处理文件夹图标
                    FileIcon = FileIcons.GetFileIcon(_filePath);
                });
            }
        }
        public string FileName
        {
            get
            {
                if (Regex.IsMatch(FilePath, @"^\w:\\$"))
                    return FilePath;
                int p = FilePath.LastIndexOf("/");
                if (p >= 0)
                {
                    return FilePath.Substring(p+1);
                }
                return Path.GetFileName(FilePath);
            }
        }
        public long Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }
        public DateTime Time
        {
            get => _time;
            set => this.RaiseAndSetIfChanged(ref _time, value);
        }

        public BitmapSource FileIcon
        {
            get => _fileIcon;
            set => this.RaiseAndSetIfChanged(ref _fileIcon, value);
        }
    }
}
