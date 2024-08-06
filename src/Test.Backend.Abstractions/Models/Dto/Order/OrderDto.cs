using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Common;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;

namespace Test.Backend.Abstractions.Models.Dto.Order
{
    public interface IUser
    {
        public UserBaseDto? User { get; set; }
    }

    public interface IAddress
    {
        public AddressBaseDto? Address { get; set; }
    }

    public interface IProduct
    {
        public ICollection<ProductDto>? Products { get; set; }
    }

    public class OrderBaseDto : BaseDto
    {
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    }

    public class OrderWithoutProductDto : OrderBaseDto, IUser, IAddress
    {
        public UserBaseDto? User { get; set; }

        public AddressBaseDto? Address { get; set; }
    }

    public class OrderWithoutAddressDto : OrderBaseDto, IUser, IProduct
    {
        public UserBaseDto? User { get; set; }

        public ICollection<ProductDto>? Products { get; set; }
    }

    public class OrderWithoutUserDto : OrderBaseDto, IAddress, IProduct
    {
        public AddressBaseDto? Address { get; set; }

        public ICollection<ProductDto>? Products { get; set; }
    }

    public class OrderDto : OrderBaseDto, IUser, IAddress, IProduct
    {
        public UserBaseDto? User { get; set; }

        public AddressBaseDto? Address { get; set; }

        public ICollection<ProductDto>? Products { get; set; }
    }
}
