namespace Test.Backend.Kafka.Options
{
    public class KafkaOptions
    {
        public string? Host { get; set; }
        public Producers? Producers { get; set; }
        public Consumers? Consumers { get; set; }
    }

    public class Producers
    {
        public string? ProducerTopic { get; set; }
        public string? ConsumerTopic { get; set; }
    }

    public class Consumers
    {
        public string? UserTopic { get; set; }
        public string? AddressTopic { get; set; }
        public string? OrderTopic { get; set; }
        public string? ProductTopic { get; set; }
    }
}
