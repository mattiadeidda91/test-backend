using Microsoft.Extensions.Logging;
using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Kafka.Interfaces;

namespace Test.Backend.Dependencies.Utils
{
    public static class HandlerExceptionUtility
    {
        public static async Task<TResponse> HandleExceptionsAsync<TResponse, TDto>(
            Func<Task<TResponse>> function,
            IEventBusService msgBus,
            string consumerTopic,
            string correlationId,
            ILogger logger)
            where TResponse : ResponseBase<TDto>, new()
            where TDto : class
        {
            try
            {
                ArgumentNullException.ThrowIfNull(function, nameof(Func<Task<TResponse>>));

                return await function.Invoke().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An unexpected exception occurred.");

                var response = new TResponse
                {
                    IsSuccess = false,
                    Dto = null,
                    Messsage = exception.Message,
                    ReturnCode = 500
                };

                await msgBus.SendMessage(response, consumerTopic, new CancellationToken(), correlationId, null);
                return response;
            }
        }
    }
}
