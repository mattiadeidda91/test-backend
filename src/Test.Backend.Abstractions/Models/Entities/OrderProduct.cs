using Test.Backend.Abstractions.Models.Entities.Common;

namespace Test.Backend.Abstractions.Models.Entities
{
    public class OrderProduct : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
        
    }
}
