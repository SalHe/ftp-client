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

        public override string ToString()
        {
            return $"{nameof(Grants)}: {Grants}, {nameof(Size)}: {Size}, {nameof(Time)}: {Time}, {nameof(Owner)}: {Owner}, {nameof(OwnerGroup)}: {OwnerGroup}";
        }
    }
}
