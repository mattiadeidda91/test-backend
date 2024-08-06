namespace Test.Backend.Kafka.Constants
{
    public static class KafkaCostants
    {
        //Kafka Activity Status Messages
        public const string ActivityInCharge = "Activity in charge";
        public const string ActivityInProgress = "Activity in progress";
        public const string ActivityFailed = "Activity failed";

        //Kafka Activity Status Details
        public const string MessageSuccessfullyPersisted = "Message successfully persisted in Kafka.";
        public const string MessagePossiblyPersisted = "Message possibly persisted, but confirmation is uncertain.";
        public const string MessageNotPersisted = "Message was not persisted in Kafka.";
        public const string MessageUnknownStatus = "Unknown status from Kafka.";
    }
}
