using FTPClient.WhyFTP;

namespace FTPClient.WhyFTP.Exceptions
{
    /// <summary>
    /// 不受支持的连接模式异常。
    /// </summary>
    public class FTPClientUnsupportedConnectionException : FTPClientException
    {
        public FTPClientUnsupportedConnectionException(string message) : base(message)
        {
        }

        public FTPClientUnsupportedConnectionException(string message, FTPResponse response) : base(message, response)
        {
        }
    }
}