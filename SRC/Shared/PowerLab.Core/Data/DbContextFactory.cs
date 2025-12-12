using Microsoft.EntityFrameworkCore;
using PowerLab.Core.Constants;

namespace PowerLab.Core.Data
{
    public static class DbContextFactory
    {
        private static DbContextOptions<PowerLabDbContext> _options;

        static DbContextFactory()
        {
            _options = 
                new DbContextOptionsBuilder<PowerLabDbContext>()
                    .UseSqlite($"Data Source={ApplicationPaths.HostDataBase}")
                    .Options;
        }

        public static PowerLabDbContext CreateDbContext()
        {
            return new PowerLabDbContext(_options);
        }
    }
}
