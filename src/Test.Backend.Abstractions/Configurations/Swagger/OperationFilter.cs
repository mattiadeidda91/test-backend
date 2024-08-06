using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Net;

namespace Test.Backend.Abstractions.Configurations.Swagger
{
    public class OperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            foreach (var item in operation.Responses)
            {
                if (item.Key == ((int)HttpStatusCode.OK).ToString(CultureInfo.CurrentCulture))
                {
                    item.Value.Description = "The request succeeded";
                }
                else if (item.Key == ((int)HttpStatusCode.BadRequest).ToString(CultureInfo.CurrentCulture))
                {
                    item.Value.Description = "The request is not valid. See the response for details";
                }
                else if (item.Key == ((int)HttpStatusCode.Unauthorized).ToString(CultureInfo.CurrentCulture))
                {
                    item.Value.Description = "The user is not owner of the specified network";
                }
                else if (item.Key == ((int)HttpStatusCode.NotFound).ToString(CultureInfo.CurrentCulture))
                {
                    item.Value.Description = "The specified network does not exist";
                }
            }

            operation.Parameters ??= new List<OpenApiParameter>();

            var apiDescription = context.ApiDescription;
            if (apiDescription.IsDeprecated())
            {
                operation.Deprecated = true;
            }
        }
    }
}
