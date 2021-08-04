using FTPClient.WhyFTP;

namespace FTPClient.WhyFTP.Exceptions
{
    public class FTPClientException : FTPException
    {
        public FTPResponse Response { get; init; }

        public FTPClientException(string message) : this(message, null)
        {
        }

        public FTPClientException(string message, FTPResponse response) : base(message)
        {
            Response = response;
        }
    }
}