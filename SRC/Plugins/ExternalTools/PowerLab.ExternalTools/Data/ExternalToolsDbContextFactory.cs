using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PowerLab.ExternalTools.Data
{
    public class ExternalToolsDbContextFactory : IDesignTimeDbContextFactory<ExternalToolsDbContext>
    {
        public ExternalToolsDbContext CreateDbContext(string[] args)
        {
            return new ExternalToolsDbContext(new PathProviderDesignTime());
        }
    }
}
