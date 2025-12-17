using Microsoft.EntityFrameworkCore;
using PowerLab.Core.Data.Entities;

namespace PowerLab.Core.Data;

public class PowerLabDbContext : DbContext
{
    public DbSet<PluginRegistryEntity> PluginRegistries { get; set; }

    public PowerLabDbContext(DbContextOptions<PowerLabDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PluginRegistryEntity>(pr => pr.OwnsOne(p => p.Manifest));
    }
}
