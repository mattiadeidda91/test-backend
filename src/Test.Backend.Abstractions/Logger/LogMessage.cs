using Microsoft.Extensions.Logging;

namespace Test.Backend.Abstractions.Logger
{
    public static partial class LogMessage
    {
        /* INFORMATION */
        [LoggerMessage(Level = LogLevel.Information, Message = "Producer successfully sent the Kafka message '{Message}'")]
        public static partial void LogInfoKafkaMessageSent(this ILogger logger, string message);

        [LoggerMessage(Level = LogLevel.Information, Message = "Consumer successfully subscribe to kafka topic")]
        public static partial void LogInfoSubscribeKafkaTopic(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Handler '{Handler}' successfully invoked")]
        public static partial void LogInfoHandlerInvoke(this ILogger logger, string handler);

        /* WARNING */

        [LoggerMessage(Level = LogLevel.Warning, Message = "Headers not found in the kafka message")]
        public static partial void LogWarningHeadersNotFound(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot retrieved event type from namespace '{ClassNamespace}'")]
        public static partial void LogWarningEventTypeNotFound(this ILogger logger, string classNamespace);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot deserialize kafka event for type '{ClassType}'")]
        public static partial void LogWarningDeserializeKafkaEvent(this ILogger logger, string classType);

        /* ERROR */

        [LoggerMessage(Level = LogLevel.Error, Message = "Error while consume kafka message by Consumer '{Message}'")]
        public static partial void LogErrorKafkaMessageSent(this ILogger logger, string message);
    }
}
