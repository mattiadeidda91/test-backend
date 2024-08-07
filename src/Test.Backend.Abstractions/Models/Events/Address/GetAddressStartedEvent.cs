using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Address.Request;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Address
{
    public class GetAddressStartedEvent : BaseEvent<AddressRequest>, IEvent
    {
    }
}
