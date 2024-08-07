using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Product
{
    public class GetProductsStartedEvent : BaseEvent<object>, IEvent
    {
    }
}
