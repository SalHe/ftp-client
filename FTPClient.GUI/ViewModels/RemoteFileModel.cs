using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace FTPClient.GUI.ViewModels
{
    public class RemoteFileModel : FileModel
    {
        private string _grants;
        private string _owner;

        public string Grants
        {
            get => _grants;
            set => this.RaiseAndSetIfChanged(ref _grants, value);
        }

        public string Owner
        {
            get => _owner;
            set => this.RaiseAndSetIfChanged(ref _owner, value);
        }
    }
}
