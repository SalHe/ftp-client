using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReactiveUI;

namespace FTPClient.GUI.ViewModels
{
    public class FileModel : ReactiveObject
    {
        private long _size;
        private DateTime _time;
        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _filePath, value);
                this.RaisePropertyChanged(nameof(FileName));
            }
        }
        public string FileName
        {
            get
            {
                if (Regex.IsMatch(FilePath, @"^\w:\\$"))
                    return FilePath;
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
    }
}
