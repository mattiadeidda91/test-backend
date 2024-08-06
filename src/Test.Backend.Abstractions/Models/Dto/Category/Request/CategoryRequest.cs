using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Common;

namespace Test.Backend.Abstractions.Models.Dto.Category.Request
{
    public class CategoryRequest : BaseDto
    {
        [Required]
        public string? Name { get; set; }
    }
}
