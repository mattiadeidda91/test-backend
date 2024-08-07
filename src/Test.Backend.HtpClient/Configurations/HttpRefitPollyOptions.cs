namespace Test.Backend.HtpClient.Configurations
{
    public class HttpRefitPollyOptions
    {
        public Dictionary<string, HttpClientOptions> Clients { get; set; } = new Dictionary<string, HttpClientOptions>();
    }
}
