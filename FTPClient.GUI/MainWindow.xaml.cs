using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FTPClient.GUI.ViewModels;
using ReactiveUI;

namespace FTPClient.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            AddTestFiles();
        }

        private void AddTestFiles()
        {
            ViewModel.LocalFiles.Add(new FileModel()
            {
                FilePath = @"C:\1.txt",
                Size = 5,
                Time = DateTime.Now
            });
            ViewModel.LocalFiles.Add(new FileModel()
            {
                FilePath = @"C:\2.txt",
                Size = 5,
                Time = DateTime.Now
            });


            ViewModel.RemoteFiles.Add(new RemoteFileModel()
            {
                FilePath = @"/home/1.txt",
                Size = 5,
                Time = DateTime.Now,
                Grants = "-rw-------",
                Owner = "ftp/ftp"
            });
            ViewModel.RemoteFiles.Add(new RemoteFileModel()
            {
                FilePath = @"/home/2.txt",
                Size = 5,
                Time = DateTime.Now,
                Grants = "-rw-------",
                Owner = "ftp/ftp"
            });

            ViewModel.TransferTasks.Add(new TransferFileModel()
            {
                FilePath = @"C:\1.txt",
                RemoteFilePath = @"/home/1.txt",
                Size = 5,
                Time = DateTime.Now,
                TransferDirection = TransferDirection.Upload,
                TransferStatus = TransferStatus.Transferring,
                Progress = 50
            });
            ViewModel.TransferTasks.Add(new TransferFileModel()
            {
                FilePath = @"C:\2.txt",
                RemoteFilePath = @"/home/2.txt",
                Size = 5,
                Time = DateTime.Now,
                TransferDirection = TransferDirection.Download,
                TransferStatus = TransferStatus.Done,
                Progress = 100
            });
        }
    }
}
