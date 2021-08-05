using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTPClient.Core;

namespace FTPClient.GUI.Cores
{
    public class FTPCoreManager
    {

        private static readonly Dictionary<string, Type> _coreTypes = new();
        private static readonly Dictionary<Type, IFTPClient> _instances = new();

        public static void RegisterCore<T>(string coreName) where T : IFTPClient, new()
        {
            if (_coreTypes.ContainsKey(coreName))
            {
                throw new ArgumentException("核心名已存在");
            }

            _coreTypes[coreName] = typeof(T);
        }

        public static void RegisterCore<T>() where T : IFTPClient, new()
        {
            RegisterCore<T>(typeof(T).Name);
        }

        public static IFTPClient GetInstance(string coreName)
        {
            if (!_coreTypes.ContainsKey(coreName))
                throw new ArgumentException("不存在这样的核心");
            Type type = _coreTypes[coreName];
            IFTPClient client;
            if (_instances.ContainsKey(type))
            {
                client = _instances[type];
            }
            else
            {
                client = (IFTPClient)Activator.CreateInstance(type);
            }
            return client;
        }

        public static IEnumerable<string> ListCoreNames()
        {
            return _coreTypes.Keys;
        }
    }
}
