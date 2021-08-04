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
using FTPClient.GUI.NLog;
using FTPClient.GUI.ViewModels;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Wrappers;
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

            // TODO 上传下载的任务调度

            InitializeNLog();
            InitializeDirectories();
        }

        private void InitializeNLog()
        {
            var target = new MyRichTextBoxTarget(richTextBox_Log);
            var asyncWrapper = new AsyncTargetWrapper { Name = "RichTextAsync", WrappedTarget = target };

            LogManager.Configuration = new LoggingConfiguration();
            LogManager.Configuration.AddTarget(asyncWrapper.Name, asyncWrapper);
            LogManager.Configuration.LoggingRules.Insert(0, new LoggingRule("*", LogLevel.Debug, asyncWrapper));
            LogManager.ReconfigExistingLoggers();

            // Logger logger = LogManager.GetCurrentClassLogger();
            // logger.Trace("Trace log");
            // logger.Debug("Debug log");
            // logger.Info("Info log");
            // logger.Warn("Warn log");
            // logger.Error("Error log");
            // logger.Fatal("Fatal log");
        }

        private void InitializeDirectories()
        {
            Task.Run(async () =>
            {
                await ViewModel.ChangeLocalDirectory(@"E:\");
                ViewModel.ChangeRemoteDirectoryToRoot();
            }).Wait();
        }
    }
}
