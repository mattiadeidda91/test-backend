using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Order.Request;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Order
{
    public class UpdateOrderStartedEvent : BaseEvent<OrderRequest>, IEvent
    {
    }
}
