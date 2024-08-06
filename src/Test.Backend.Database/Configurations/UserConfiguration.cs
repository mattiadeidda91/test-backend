//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Test.Backend.Abstractions.Models.Entities;
//using Test.Backend.Database.Configurations.Common;

//namespace Test.Backend.Database.Configurations
//{
//    public class UserConfiguration : BaseEntityConfiguration<User>
//    {
//        public override void Configure(EntityTypeBuilder<User> builder)
//        {
//            base.Configure(builder);

//            builder.HasMany(u => u.Orders)
//                .WithOne(o => o.User)
//                .HasForeignKey(o => o.UserId)
//                .OnDelete(DeleteBehavior.Restrict);
//        }
//    }
//}
