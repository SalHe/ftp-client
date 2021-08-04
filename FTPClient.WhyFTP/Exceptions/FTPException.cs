using System;

namespace FTPClient.WhyFTP.Exceptions
{
    public class FTPException : Exception
    {
        public FTPException(string message) : base(message)
        {
        }
    }
}