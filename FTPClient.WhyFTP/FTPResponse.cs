
namespace FTPClient.WhyFTP
{
    public class FTPResponse
    {
        public FTPStatusCode StatusCode { get; init; }
        public string Body { get; init; }
        public string Response { get; init; }

        public static FTPResponse Parse(string response)
        {
            // int pos = 0;
            // while (response[pos] != ' ')
            // {
            // }
            int pos = 3;

            string statusCode = response.Substring(0, pos);
            string body = response.Substring(pos);
            return new FTPResponse
            {
                StatusCode = (FTPStatusCode) int.Parse(statusCode),
                Body = body,
                Response = response
            };
        }
    }
}