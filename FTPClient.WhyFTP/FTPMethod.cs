namespace FTPClient.WhyFTP
{
    public class FTPMethod
    {
        public const string DownloadFile = "RETR";
        public const string ListDirectory = "NLST";
        public const string UploadFile = "STOR";
        public const string DeleteFile = "DELE";
        public const string AppendFile = "APPE";
        public const string GetFileSize = "SIZE";
        public const string UploadFileWithUniqueName = "STOU";
        public const string MakeDirectory = "MKD";
        public const string RemoveDirectory = "RMD";
        public const string ListDirectoryDetails = "LIST";
        public const string GetDateTimestamp = "MDTM";
        public const string PrintWorkingDirectory = "PWD";
        public const string Rename = "RENAME";
    }
}