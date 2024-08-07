using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Address
{
    public class GetAddressesStartedEvent : BaseEvent<object>, IEvent
    {
    }
}
