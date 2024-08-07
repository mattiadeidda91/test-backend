using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Events.Common;
using Test.Backend.Kafka.Models;

namespace Test.Backend.Kafka.Interfaces
{
    public interface IEventBusService
    {
        Task<DeliveryResult<string, T>> SendMessage<T>(T message, string topic, CancellationToken cancellationToken, string? key = null, Headers? headers = null);
        Task Subscribe<T>(Action<ConsumeResult<string, T>> action, string topic, string? groupId =null);
        Task Subscribe(Action<ConsumeResult<string, string>> action, string topic, string? groupId = null);
        Task<T?> ConsumeAsync<T>(string topic, string key, string? groupId = null) where T : class;
        (TEvent, Headers) GenerateMsgBusEvent<TEvent, TActivity>(TActivity activity, string correlationId) where TEvent : BaseEvent<TActivity>, new() where TActivity : class;
        public ActionResult<MessageBusResponse<TDto>> ManageBusResponse<TEvent, TDto>(ControllerBase controller, DeliveryResult<string, TEvent> deliveryResult, TDto? activityRequest)
                where TDto : BaseDto
                where TEvent : BaseEvent<TDto>;
        Task<TResponse?> HandleMsgBusMessages<TEvent, TActivity, TResponse>(
            TActivity request,
            string userTopic,
            string consumerTopic,
            CancellationToken cancellationToken)
            where TEvent : BaseEvent<TActivity>, new()
            where TActivity : class
            where TResponse : class;
    }
}