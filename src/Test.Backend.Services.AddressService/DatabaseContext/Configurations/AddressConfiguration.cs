using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Configurations.Common;

namespace Test.Backend.Services.AddressService.DatabaseContext.Configurations
{
    public class AddressConfiguration : BaseEntityConfiguration<Address>
    {
        public override void Configure(EntityTypeBuilder<Address> builder)
        {
            base.Configure(builder);

            builder.ToTable("Addresses");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.City).HasMaxLength(100).IsRequired().IsUnicode(false);
            builder.Property(u => u.Street).HasMaxLength(100).IsRequired().IsUnicode(false);
            builder.Property(u => u.PostalCode).HasMaxLength(100).IsRequired().IsUnicode(false);
            builder.Property(u => u.Country).HasMaxLength(100).IsRequired().IsUnicode(false);

            //builder.HasMany(a => a.Orders)
            //    .WithOne(o => o.DeliveryAddress)
            //    .HasForeignKey(o => o.DeliveryAddressId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
