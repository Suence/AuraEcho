using Microsoft.EntityFrameworkCore.Design;

namespace AuraEcho.ExternalTools.Data;

public class ExternalToolsDbContextFactory : IDesignTimeDbContextFactory<ExternalToolsDbContext>
{
    public ExternalToolsDbContext CreateDbContext(string[] args)
    {
        return new ExternalToolsDbContext(new PathProviderDesignTime());
    }
}
