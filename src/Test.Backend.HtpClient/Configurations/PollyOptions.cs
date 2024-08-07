namespace Test.Backend.HtpClient.Configurations
{
    public class PollyOptions
    {
        public bool RetryPolicyEnable { get; set; }
        public int RetryCount { get; set; } = 3; // Default retry count
    }
}
