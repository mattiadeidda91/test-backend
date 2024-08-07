using Refit;

namespace Test.Backend.Services.OrderService.Utils
{
    public static class UtilityClient
    {
        public static async Task<Dictionary<Guid, TDto>> FetchEntitiesAsync<TDto>(
            IEnumerable<Guid> ids,
            Func<Guid, Task<ApiResponse<TDto>>> fetchEntity,
            CancellationToken cancellationToken)
            where TDto : class
        {
            var dtos = new Dictionary<Guid, TDto>();

            var tasks = ids.Select(async id =>
            {
                var response = await fetchEntity(id);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    dtos[id] = response.Content;
                }
            });

            await Task.WhenAll(tasks);

            return dtos;
        }

        public static async Task<TDto?> FetchEntityAsync<TDto>(
            Guid id,
            Func<Guid, Task<ApiResponse<TDto>>> fetchEntity,
            CancellationToken cancellationToken)
            where TDto : class
        {
            var response = await fetchEntity(id);

            return response.IsSuccessStatusCode && response.Content != null ? response.Content : null;
        }
    }
}
