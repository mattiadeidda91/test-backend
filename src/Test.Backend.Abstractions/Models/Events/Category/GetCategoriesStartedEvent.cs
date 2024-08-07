using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Category
{
    public class GetCategoriesStartedEvent : BaseEvent<object>, IEvent
    {
    }
}
