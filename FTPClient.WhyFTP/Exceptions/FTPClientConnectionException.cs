using FTPClient.WhyFTP;

namespace FTPClient.WhyFTP.Exceptions
{
    /// <summary>
    /// 连接异常。
    /// </summary>
    public class FTPClientConnectionException : FTPClientException
    {
        public FTPClientConnectionException(string message) : base(message)
        {
        }

        public FTPClientConnectionException(string message, FTPResponse response) : base(message, response)
        {
        }
    }
}