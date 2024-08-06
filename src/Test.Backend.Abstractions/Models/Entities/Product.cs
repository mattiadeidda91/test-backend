using Test.Backend.Abstractions.Models.Entities.Common;

namespace Test.Backend.Abstractions.Models.Entities
{
    public class Product : BaseEntity
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
