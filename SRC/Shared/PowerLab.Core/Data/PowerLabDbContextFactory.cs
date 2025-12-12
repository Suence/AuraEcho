using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PowerLab.Core.Constants;

namespace PowerLab.Core.Data;

public class PowerLabDbContextFactory : IDesignTimeDbContextFactory<PowerLabDbContext>
{
    public PowerLabDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PowerLabDbContext>();

        optionsBuilder.UseSqlite($"Data Source={ApplicationPaths.HostDataBase}");
        return new PowerLabDbContext(optionsBuilder.Options);
    }
}
