using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;
using System.Net.Mime;

namespace Test.Backend.Abstractions.Configurations.Swagger
{
    public class ProducesResponseTypeConvention :
      IApplicationModelConvention
    {
        private readonly List<Type> voidActionResults = new List<Type>() { typeof(void), typeof(Task) };
        public void Apply(ApplicationModel application)
        {
            ArgumentNullException.ThrowIfNull(application);

            var actions = application.Controllers.SelectMany(sm => sm.Actions).ToList();
            foreach (var action in actions)
            {
                if (voidActionResults.Contains(action.ActionMethod.ReturnType))
                {
                    action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.NoContent));
                }
                else
                {
                    var httpMethodAttribute = action.Attributes.OfType<HttpMethodAttribute>().FirstOrDefault();
                    switch (httpMethodAttribute?.HttpMethods.FirstOrDefault())
                    {
                        case "GET":

                            if (!action.ActionMethod.ReturnType.IsAssignableFrom(typeof(Enumerable)))
                            {
                                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.NotFound));
                            }
                            break;
                        case "POST":
                            break;
                        case "PUT":

                            break;
                        case "DELETE":

                            break;
                    }
                }
                action.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.OK));
                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.BadRequest));
                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.Unauthorized));
                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.Forbidden));
                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.UnprocessableEntity));
                action.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.InternalServerError));
            }
        }
    }
}
