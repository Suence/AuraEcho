using Microsoft.EntityFrameworkCore;
using AuraEcho.ExternalTools.Models;
using AuraEcho.PluginContracts.Interfaces;
using System.IO;

namespace AuraEcho.ExternalTools.Data;

public class ExternalToolsDbContext : DbContext
{
    private readonly IPathProvider _pathProvider;
    public DbSet<ExternalTool> ExternalTools { get; set; }

    public ExternalToolsDbContext(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(_pathProvider.DataRootPath, "ExternalTools", "externaltools.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
