using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace FTPClient.GUI.ViewModels
{
    public class FileModel : ReactiveObject
    {
        private int _size;
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
        // TODO 获取文件名
        public string FileName => FilePath;

        public int Size
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
