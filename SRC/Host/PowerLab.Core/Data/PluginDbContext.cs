using System.IO;
using Microsoft.EntityFrameworkCore;
using PowerLab.Core.Constants;
using PowerLab.Core.Models;
namespace PowerLab.Core.Data
{
    public class PluginDbContext : DbContext
    {
        public DbSet<PluginRegistry> PluginRegistries { get; set; }

        //public PluginDbContext(DbContextOptions<PluginDbContext> options) : base(options)
        //{
        //}

        public PluginDbContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var dbPath = Path.Combine(ApplicationPaths.Data, "powerlab.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PluginRegistry>(pr => pr.OwnsOne(p => p.Manifest));
        }
    }
}
