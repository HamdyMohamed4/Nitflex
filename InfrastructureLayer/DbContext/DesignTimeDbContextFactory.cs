using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InfrastructureLayer
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NetflixContext>
    {
        public NetflixContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NetflixContext>();
            optionsBuilder.UseSqlServer(
               "Server=.;Database=NetflixDatabase;User Id=SA;Password=^r@2w6x4g8&#M#&*;TrustServerCertificate=True;"
            );

            return new NetflixContext(optionsBuilder.Options);
        }
    }
}
