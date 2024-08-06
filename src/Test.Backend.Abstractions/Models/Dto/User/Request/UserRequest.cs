using System.ComponentModel.DataAnnotations;
using Test.Backend.Abstractions.Models.Dto.Common;

namespace Test.Backend.Abstractions.Models.Dto.User.Request
{
    public class UserRequest : BaseDto
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Email { get; set; }
    }
}
