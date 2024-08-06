namespace Test.Backend.Kafka.Models
{
    public class MessageBusResponse<T> where T : class
    {
        public Guid ActivityId { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Detail { get; set; }
        public T? OriginalPayload { get; set; }
    }
}
