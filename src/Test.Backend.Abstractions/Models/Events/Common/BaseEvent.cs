namespace Test.Backend.Abstractions.Models.Events.Common
{
    public abstract class BaseEvent<T> where T : class
    {
        public Guid ActivityId { get; set; }
        public T? Activity { get; set; }
        public string? CorrelationId { get; set; }
    }
}
