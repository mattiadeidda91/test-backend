using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Common;

namespace Test.Backend.Abstractions.Models.Dto.Address.Request
{
    public class AddressRequest : BaseDto
    {
        [Required]
        public string? Street { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? PostalCode { get; set; }

        [Required]
        public string? Country { get; set; }
    }
}
