using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Category.Request;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Category
{
    public class GetCategoryStartedEvent : BaseEvent<CategoryRequest>, IEvent
    {
    }
}
