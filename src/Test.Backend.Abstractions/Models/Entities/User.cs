using Test.Backend.Abstractions.Models.Entities.Common;

namespace Test.Backend.Abstractions.Models.Entities
{
    public class User : BaseEntity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
