using Microsoft.AspNetCore.Builder;
using Test.Backend.Abstractions.Middleware;

namespace Test.Backend.Abstractions.Extensions
{
    public static class UseExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
