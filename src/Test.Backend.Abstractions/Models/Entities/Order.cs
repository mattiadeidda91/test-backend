using Test.Backend.Abstractions.Models.Entities.Common;

namespace Test.Backend.Abstractions.Models.Entities;
public class Order : BaseEntity
{
    public DateTime OrderDate { get; set; }
    public Guid UserId { get; set; }
    public Guid DeliveryAddressId { get; set; }

    public virtual User? User { get; set; }
    public virtual Address? DeliveryAddress { get; set; }
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}

