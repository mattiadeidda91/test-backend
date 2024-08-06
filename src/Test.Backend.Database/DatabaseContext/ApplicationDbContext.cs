using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Entities.Common;
using Test.Backend.Database.DatabaseContext.Common;

#pragma warning disable S2219 // Runtime type checking should be simplified

namespace Test.Backend.Database.DatabaseContext
{
    public class ApplicationDbContext : BaseDbContext, IApplicationDbContext
    {
        //public DbSet<Order> Orders { get; set; } = null!;
        //public DbSet<Product> Products { get; set; } = null!;
        //public DbSet<OrderProduct> OrderProducts { get; set; } = null!;
        //public DbSet<User> Users { get; set; } = null!;
        //public DbSet<Category> Categories { get; set; } = null!;
        //public DbSet<Address> Addresses { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    //Get All EntityConfigurations
        //    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        //    base.OnModelCreating(modelBuilder);
        //}

        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    var entries = ChangeTracker.Entries()
        //        .Where(e => typeof(IEntity).IsAssignableFrom(e.Entity.GetType()));

        //    var currentTime = DateTime.UtcNow;

        //    foreach (var entry in entries.Where(e => e.State is EntityState.Added or EntityState.Modified))
        //    {
        //        if (entry.State is EntityState.Added)
        //        {
        //            entry.Property("InsertDate").CurrentValue = currentTime;
        //        }
        //        else
        //        {
        //            entry.Property("InsertDate").IsModified = false;
        //        }

        //        entry.Property("UpdateDate").CurrentValue = currentTime;
        //    }

        //    return base.SaveChangesAsync(cancellationToken);
        //}
    }
}

#pragma warning restore S2219 // Runtime type checking should be simplified
