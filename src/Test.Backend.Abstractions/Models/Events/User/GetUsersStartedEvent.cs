using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.User
{
    public class GetUsersStartedEvent : BaseEvent<object>, IEvent
    {
    }
}
