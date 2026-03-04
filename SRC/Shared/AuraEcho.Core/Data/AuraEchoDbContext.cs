using Microsoft.EntityFrameworkCore;
using AuraEcho.Core.Data.Entities;

namespace AuraEcho.Core.Data;

public class AuraEchoDbContext : DbContext
{
    public DbSet<PluginRegistryEntity> PluginRegistries { get; set; }

    public AuraEchoDbContext(DbContextOptions<AuraEchoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PluginRegistryEntity>(pr => pr.OwnsOne(p => p.Manifest));
    }
}
