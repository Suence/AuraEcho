using Microsoft.EntityFrameworkCore;
using PowerLab.Core.Models;
namespace PowerLab.Core.Data;

public class PowerLabDbContext : DbContext
{
    public DbSet<PluginRegistry> PluginRegistries { get; set; }

    public PowerLabDbContext(DbContextOptions<PowerLabDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PluginRegistry>(pr => pr.OwnsOne(p => p.Manifest));
    }
}
