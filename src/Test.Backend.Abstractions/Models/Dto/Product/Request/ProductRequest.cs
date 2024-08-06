using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Common;

namespace Test.Backend.Abstractions.Models.Dto.Product.Request
{
    public class ProductRequest : BaseDto
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
