using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Configurations.Common;

namespace Test.Backend.Services.OrderService.DatabaseContext.Configurations
{
    public class OrderProductConfiguration : BaseEntityConfiguration<OrderProduct>
    {
        public override void Configure(EntityTypeBuilder<OrderProduct> builder)
        {
            base.Configure(builder);

            builder.HasKey(op => new { op.OrderId, op.ProductId });

            builder.HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(op => op.Product)
            //    .WithMany(p => p.OrderProducts)
            //    .HasForeignKey(op => op.ProductId);
        }
    }
}
