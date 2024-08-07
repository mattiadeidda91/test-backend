using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Dto.Product.Request;
using Test.Backend.Abstractions.Models.Events.Common;

namespace Test.Backend.Abstractions.Models.Events.Product
{
    public class CreateProductStartedEvent : BaseEvent<ProductRequest>, IEvent
    {
    }
}
