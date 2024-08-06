using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Common;

namespace Test.Backend.Abstractions.Models.Dto.Order.Request
{
    public class OrderRequest : BaseDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid DeliveryAddressId { get; set; }

        [Required]
        public IEnumerable<Guid>? ProductIds { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
