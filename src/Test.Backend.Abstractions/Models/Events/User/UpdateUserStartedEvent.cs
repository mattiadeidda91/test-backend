using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.User.Request;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.User
{
    public class UpdateUserStartedEvent : BaseEvent<UserRequest>, IEvent
    {
    }
}
