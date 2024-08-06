using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;
using Test.Backend.Abstractions.Logger;
using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Events.Common;
using Test.Backend.Kafka.Constants;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Models;
using Test.Backend.Kafka.Options;
using Test.Backend.Kafka.Utils;

namespace Test.Backend.Kafka.Services
{
    public class EventBusService : IEventBusService
    {
        private readonly ClientConfig producerConfig;
        private readonly KafkaOptions kafkaOptions;
        private readonly ILogger<EventBusService> logger;

        public EventBusService(IOptions<KafkaOptions> options, ILogger<EventBusService> logger)
        {
            this.logger = logger;
            kafkaOptions = options.Value;
            producerConfig = new ProducerConfig()
            {
                BootstrapServers = options.Value.Host,
                SecurityProtocol = SecurityProtocol.Plaintext
            };
        }

        private ConsumerConfig GetConsumerConfig(string? groupId)
        {
            return new ConsumerConfig()
            {
                BootstrapServers = kafkaOptions.Host,
                SecurityProtocol = SecurityProtocol.Plaintext,
                GroupId = string.IsNullOrWhiteSpace(groupId) ? Guid.Empty.ToString() : groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest

            };
        }

        public async Task<DeliveryResult<string, T>> SendMessage<T>(T message, string topic, CancellationToken cancellationToken, string? key = null, Headers? headers = null)
        {
            using var producer = new ProducerBuilder<string, T>(producerConfig)
                .SetValueSerializer(new JsonSerializer<T>())
                .Build();

            var kMessage = new KafkaMessage<T>(message);
            kMessage.Key = key;
            kMessage.Headers = headers;

            var ret = await producer.ProduceAsync(topic, kMessage, cancellationToken);

            logger.LogInfoKafkaMessageSent(JsonSerializer.Serialize(kMessage));

            producer.Flush();

            return ret;
        }

        public async Task<T?> ConsumeAsync<T>(string topic, string key, string? groupId = null) where T : class
        {
            using var consumer = new ConsumerBuilder<string, string>(GetConsumerConfig(groupId)).Build();

            consumer.Subscribe(topic);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var cr = consumer.Consume(cts.Token);
                    if (cr.Message.Key == key)
                    {
                        T? response = JsonSerializer.Deserialize<T>(cr.Message.Value);
                        return response;
                    }
                }
            }
            catch (ConsumeException e)
            {
                // Log and handle the exception
                throw new Exception($"Error consuming message: {e.Message}");
            }
            finally
            {
                consumer.Close();
            }

            return null;
        }

        public async Task Subscribe<T>(Action<ConsumeResult<string, T>> action, string topic, string? groupId = null)
        {
            using var consumer = new ConsumerBuilder<string, T>(GetConsumerConfig(groupId))
                .SetValueDeserializer(new JsonSerializer<T>())
                .Build();

            consumer.Subscribe(topic);

            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(default(CancellationToken));
                        action(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        logger.LogErrorKafkaMessageSent(ex.Message);
                    }
                }
            });

        }

        public async Task Subscribe(Action<ConsumeResult<string, string>> action, string topic, string? groupId = null)
        {
            using var consumer = new ConsumerBuilder<string, string>(GetConsumerConfig(groupId)).Build();

            consumer.Subscribe(topic);

            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(default(CancellationToken));
                        action(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        logger.LogErrorKafkaMessageSent(ex.Message);
                    }
                }
            });
        }

        public (TEvent, Headers) GenerateMsgBusEvent<TEvent, TActivity>(TActivity activity, string correlationId)
            where TEvent : BaseEvent<TActivity>, new() where TActivity : class
        {
            var msgBusEvent = new TEvent
            {
                ActivityId = Guid.NewGuid(),
                Activity = activity,
                CorrelationId = correlationId
            };

            var msgBusHeaders = new Headers
            {
                { "namespace", Encoding.UTF8.GetBytes(typeof(TEvent).FullName!) }
            };

            return (msgBusEvent, msgBusHeaders);
        }

        public ActionResult<MessageBusResponse<TDto>> ManageBusResponse<TEvent, TDto>(ControllerBase controller, DeliveryResult<string, TEvent> deliveryResult, TDto? activityRequest)
                where TDto : BaseDto
                where TEvent : BaseEvent<TDto>
        {
            ArgumentNullException.ThrowIfNull(controller);
            ArgumentNullException.ThrowIfNull(activityRequest);

            return deliveryResult.Status switch
            {
                PersistenceStatus.Persisted => controller.Ok(new MessageBusResponse<TDto>
                {
                    ActivityId = activityRequest.Id,
                    Title = KafkaCostants.ActivityInCharge,
                    Status = (int)HttpStatusCode.OK,
                    Detail = KafkaCostants.MessageSuccessfullyPersisted,
                    OriginalPayload = activityRequest
                }),

                PersistenceStatus.PossiblyPersisted => controller.StatusCode((int)HttpStatusCode.Accepted, new MessageBusResponse<TDto>
                {
                    ActivityId = activityRequest.Id,
                    Title = KafkaCostants.ActivityInProgress,
                    Status = (int)HttpStatusCode.Accepted,
                    Detail = KafkaCostants.MessagePossiblyPersisted,
                    OriginalPayload = activityRequest
                }),

                PersistenceStatus.NotPersisted => controller.BadRequest(new MessageBusResponse<TDto>
                {
                    ActivityId = activityRequest.Id,
                    Title = KafkaCostants.ActivityFailed,
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = KafkaCostants.MessageNotPersisted,
                    OriginalPayload = activityRequest
                }),

                _ => controller.BadRequest(new MessageBusResponse<TDto>
                {
                    ActivityId = activityRequest.Id,
                    Title = KafkaCostants.ActivityFailed,
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = KafkaCostants.MessageUnknownStatus,
                    OriginalPayload = activityRequest
                }),
            };
        }
    }
}