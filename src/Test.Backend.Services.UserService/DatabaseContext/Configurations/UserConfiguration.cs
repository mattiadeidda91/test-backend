using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Configurations.Common;

namespace Test.Backend.Services.UserService.DatabaseContext.Configurations
{
    public class UserConfiguration : BaseEntityConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired().IsUnicode(false);
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired().IsUnicode(false);
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired().IsUnicode(false);
            builder.Property(u => u.Email).HasMaxLength(100).IsRequired().IsUnicode(false);

            //builder.HasMany(u => u.Orders)
            //    .WithOne(o => o.User)
            //    .HasForeignKey(o => o.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
