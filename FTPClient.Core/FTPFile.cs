using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient.Core
{
    public class FTPFile
    {
        /// <summary>
        /// 文件完整路径。
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 权限。-rw-------
        /// </summary>
        public string Grants { get; set; }

        /// <summary>
        /// 文件大小。
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 修改时间。
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 拥有者。
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// 拥有者用户组别。
        /// </summary>
        public string OwnerGroup { get; set; }

        public bool IsDirectory => Grants.StartsWith("d");

        public override string ToString()
        {
            return $"{nameof(Grants)}: {Grants}, {nameof(Size)}: {Size}, {nameof(Time)}: {Time}, {nameof(Owner)}: {Owner}, {nameof(OwnerGroup)}: {OwnerGroup}";
        }

        /// <summary>
        /// 从FTP服务器LIST命令返回的一行数据中解析出对应的文件信息。
        /// 
        /// </summary>
        /// <param name="os">服务器操作系统类型</param>
        /// <param name="line">一行信息</param>
        /// <param name="baseDir">所在目录完整路径</param>
        /// <returns>对应的FTPFile实例</returns>
        public static FTPFile FromFTPListLine(OperationSystemType os, string line, string baseDir)
        {
            FTPFile file = null;
            if (os == OperationSystemType.Linux || os == OperationSystemType.Unix)
            {
                file = new FTPFile()
                {
                    Grants = line.Substring(0, 10),
                    Owner = line.Substring(16, 8).Trim(),
                    OwnerGroup = line.Substring(25, 8).Trim(),
                    Size = int.Parse(line.Substring(34, 8)),
                    Time = DateTime.Parse(line.Substring(43, 12)),
                    FilePath = baseDir + "/" + line.Substring(56)
                };
                if (file.IsDirectory) file.Size = -1;
                return file;
            }
            else if (os == OperationSystemType.Windows)
            {
                file = new FTPFile()
                {
                    Grants = line.Substring(0, 10),
                    Owner = line.Substring(13, 3).Trim(),
                    OwnerGroup = line.Substring(13, 3).Trim(),
                    Size = int.Parse(line.Substring(21, 14)),
                    Time = DateTime.Parse(line.Substring(36, 12)),
                    FilePath = baseDir + "/" + line.Substring(49)
                };
                if (file.IsDirectory) file.Size = -1;
                return file;
            }
            throw new ArgumentException("无效的操作系统类型");
        }
    }
}
