using Test.Backend.Abstractions.Models.Entities.Common;

namespace Test.Backend.Abstractions.Models.Entities
{
    public class Address : BaseEntity
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
