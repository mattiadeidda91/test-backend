using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Order
{
    public class GetOrdersStartedEvent : BaseEvent<object>, IEvent
    {
    }
}
