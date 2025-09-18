using Microsoft.EntityFrameworkCore;
using PowerLab.Core.Data;

namespace PowerLab.DataMigrator
{
    public static class Program
    {
        public static void Main()
        {
            using var powerLabDbContext = new PowerLabDbContext();
            if (!powerLabDbContext.Database.GetPendingMigrations().Any()) return;

            powerLabDbContext.Database.Migrate();
        }
    }
}
