﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using FTPClient.Core;
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

        private Thread _transferringThread;
        private IList<Thread> _addTaskThreads = new List<Thread>();

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            InitializeNLog();
            InitializeDirectories();
            InitializeTransferring();
        }

        private void InitializeTransferring()
        {
            // 因为只有一个数据通道完成数据传输
            // 只能一个任务一个任务的处理

            Semaphore hasTaskSemaphore = new Semaphore(0, 1);
            Semaphore noTaskTransferringSpSemaphore = new Semaphore(1, 1);
            TransferFileModel transferringFile = null;

            // 用于收集传输任务的代码
            ViewModel.TransferTasks.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    Thread thread = new Thread(th =>
                    {
                        foreach (TransferFileModel task in args.NewItems!)
                        {
                            noTaskTransferringSpSemaphore.WaitOne();
                            transferringFile = task;
                            hasTaskSemaphore.Release();
                        }

                        _addTaskThreads.Remove((Thread)th);
                    });
                    thread.IsBackground = true;
                    thread.Start(thread);
                }
            };

            void ReportProgress(float percent, bool failed, bool finished)
            {
                if (failed)
                    transferringFile.TransferStatus = TransferStatus.Error;
                else if (finished)
                    transferringFile.TransferStatus = TransferStatus.Done;
                else
                    transferringFile.TransferStatus = TransferStatus.Transferring;

                transferringFile.Progress = (int)(percent * 100);
            }

            // 用于调度传输的代码
            _transferringThread = new Thread(() =>
            {
                while (hasTaskSemaphore.WaitOne())
                {
                    if (transferringFile.TransferDirection == TransferDirection.Upload)
                        ViewModel.FTPClient.UploadFile(transferringFile.FilePath, transferringFile.RemoteFilePath,
                            ReportProgress);
                    else if (transferringFile.TransferDirection == TransferDirection.Download)
                        ViewModel.FTPClient.DownloadFile(transferringFile.RemoteFilePath, transferringFile.FilePath,
                            ReportProgress);
                    noTaskTransferringSpSemaphore.Release();
                }
            });
            _transferringThread.IsBackground = true;
            _transferringThread.Start();
        }

        private void InitializeViewModel()
        {
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
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

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = ViewModel.Connected
                       && MessageBoxResult.No == MessageBox.Show("当前还未断开服务器，是否直接退出？", "退出", MessageBoxButton.YesNo, MessageBoxImage.Error);
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            // TODO 退出时结束所有后台线程
            // foreach (Thread addTaskThread in _addTaskThreads)
            // {
            //     addTaskThread.Interrupt();
            // }
            // _transferringThread.Interrupt();
        }
    }
}
