using System.IO;
using System.Net.Sockets;
using System.Text;
using NLog;

namespace FTPClient.WhyFTP
{
    public class FTPSocket : Socket
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FTPSocket(SafeSocketHandle handle) : base(handle)
        {
        }

        public FTPSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(
            addressFamily, socketType, protocolType)
        {
        }

        public FTPSocket(SocketInformation socketInformation) : base(socketInformation)
        {
        }

        public FTPSocket(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }

        public byte[] ReadAllBytes()
        {
            byte[] buffer = new byte[256];
            var all = new MemoryStream();
            int bytes;
            do
            {
                bytes = Receive(buffer);
                all.Write(buffer);
            } while (bytes >= buffer.Length);

            return all.ToArray();
        }

        public string ReadLine()
        {
            byte[] buffer = new byte[1];
            StringBuilder sb = new StringBuilder();
            do
            {
                if (Receive(buffer) <= 0) break;
                if (buffer[0] != '\r' && buffer[0] != '\n')
                    sb.Append((char) buffer[0]);
            } while (buffer[0] != '\n');

            return sb.ToString();
        }

        public FTPResponse ReadResponse()
        {
            var line = ReadLine();
            _logger.Debug(line);
            return FTPResponse.Parse(line);
        }

        public void WriteLine(string line)
        {
            Send(Encoding.ASCII.GetBytes(line));
            Send(new[] {(byte) '\r', (byte) '\n'});
        }

        public FTPResponse Request(string method, params string[] args)
        {
            StringBuilder request = new StringBuilder(method);
            foreach (var arg in args)
            {
                request.Append(" ");
                request.Append(arg);
            }

            return Request(request.ToString());
        }

        public FTPResponse Request(string command)
        {
            _logger.Debug(command);
            WriteLine(command);
            return ReadResponse();
        }
    }
}