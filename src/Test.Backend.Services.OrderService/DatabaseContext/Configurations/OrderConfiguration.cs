using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Configurations.Common;

namespace Test.Backend.Services.OrderService.DatabaseContext.Configurations
{
    public class OrderConfiguration : BaseEntityConfiguration<Order>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);

            builder.ToTable("Orders");

            builder.Property(u => u.UserId).IsRequired();
            builder.Property(u => u.DeliveryAddressId).IsRequired();

            builder.HasMany(o => o.OrderProducts)
               .WithOne(op => op.Order)
               .HasForeignKey(op => op.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(o => o.User)
            //    .WithMany(u => u.Orders)
            //    .HasForeignKey(o => o.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(o => o.DeliveryAddress)
            //    .WithMany(a => a.Orders)
            //    .HasForeignKey(o => o.DeliveryAddressId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
