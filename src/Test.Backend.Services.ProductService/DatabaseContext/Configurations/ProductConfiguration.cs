using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Configurations.Common;

namespace Test.Backend.Services.ProductService.DatabaseContext.Configurations
{
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            builder.ToTable("Products");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Price).IsRequired();
            builder.Property(u => u.Name).HasMaxLength(100).IsRequired().IsUnicode(false);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
        }
    }
}
