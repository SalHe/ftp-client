using FTPClient.WhyFTP;

namespace FTPClient.WhyFTP.Exceptions
{
    /// <summary>
    /// 动作异常。
    /// </summary>
    public class FTPClientActionException : FTPClientException
    {
        public FTPClientActionException(string message) : base(message)
        {
        }

        public FTPClientActionException(string message, FTPResponse response) : base(message, response)
        {
        }
    }
}