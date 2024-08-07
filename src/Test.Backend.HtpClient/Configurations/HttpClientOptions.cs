namespace Test.Backend.HtpClient.Configurations
{
    public class HttpClientOptions
    {
        public string Name { get; set; } = string.Empty;
        public string BaseAddress { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public PollyOptions PollyOptions { get; set; } = new PollyOptions();
    }
}
