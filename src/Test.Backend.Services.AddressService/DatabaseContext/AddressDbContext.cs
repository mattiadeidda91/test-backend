﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Abstractions.Models.Entities.Common;
using Test.Backend.Database.DatabaseContext.Common;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.AddressService.DatabaseContext.Configurations;

public class AddressDbContext : BaseDbContext, IAddressDbContext
{
    public DbSet<Address> Addresses { get; set; } = null!;

    public AddressDbContext(DbContextOptions<AddressDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Get All EntityConfigurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(IEntity).IsAssignableFrom(e.Entity.GetType()));

        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries.Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (entry.State is EntityState.Added)
            {
                entry.Property("InsertDate").CurrentValue = currentTime;
            }
            else
            {
                entry.Property("InsertDate").IsModified = false;
            }

            entry.Property("UpdateDate").CurrentValue = currentTime;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
