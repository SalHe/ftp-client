namespace FTPClient.WhyFTP
{
    public record FTPCredential
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }
}