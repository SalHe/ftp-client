using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient.Core
{
    public interface IFTPClient
    {
        /// <summary>
        /// 报告进度的委托定义。
        /// </summary>
        /// <param name="percent">百分比(0~1)</param>
        /// <param name="failed">是否传输失败。用于指示传输过程是否被终止。</param>
        /// <param name="finished">传输完成。用于只是传输是否正常结束。</param>
        public delegate void ReportProgress(float percent, bool failed, bool finished);

        /// <summary>
        /// 是否已连接到服务器
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// 初始化FTP客户端。
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        void Init(string host, int port, string username, string password);

        /// <summary>
        /// 连接FTP服务器。
        /// </summary>
        void Connect();

        /// <summary>
        /// 断开连接。
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 改变当前目录。
        /// </summary>
        /// <param name="path">目录路径</param>
        void ChangeDirectory(string path);

        /// <summary>
        /// 删除目录。
        /// </summary>
        /// <param name="path">目录路径</param>
        void DeleteDirectory(string path);

        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="file">文件路径</param>
        void DeleteFile(string file);

        /// <summary>
        /// 重命名文件。
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <param name="newFilePath">新文件完整路径</param>
        void RenameFile(string filePath, string newFilePath);

        /// <summary>
        /// 获取当前目录。
        /// </summary>
        /// <returns>当前目录</returns>
        string GetCurrentDirectory();

        /// <summary>
        /// 获取文件列表。
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>文件信息</returns>
        FTPFile[] ListFiles(string path);

        /// <summary>
        /// 上传文件。
        /// </summary>
        /// <param name="localPath">本地路径</param>
        /// <param name="remotePath">远程路径</param>
        /// <param name="reportProgressFunction">进度回馈回调</param>
        void UploadFile(string localPath, string remotePath, ReportProgress reportProgressFunction);

        /// <summary>
        /// 下载文件。
        /// </summary>
        /// <param name="remotePath">远程路径</param>
        /// <param name="localPath">本地路径</param>
        /// <param name="reportProgressFunction">进度回馈回调</param>
        void DownloadFile(string remotePath, string localPath, ReportProgress reportProgressFunction);

    }

}
