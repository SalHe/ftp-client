using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FTPClient.Core;
using FTPClient.Core.XyxFTP;
using FTPClient.GUI.Cores;
using FTPClient.GUI.ViewModels;
using FTPClient.WhyFTP;

namespace FTPClient.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FTPCoreManager.RegisterCore<FakeFTPClient>("虚拟FTP客户端(用于测试UI)");
            FTPCoreManager.RegisterCore<WhyFTPClient>("Why FTP");
            FTPCoreManager.RegisterCore<XyxFTPClient>(MainViewModel.DefaultFTPCore);
        }
    }
}
