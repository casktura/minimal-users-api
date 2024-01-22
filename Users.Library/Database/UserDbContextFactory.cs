using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using Users.Library.Constants;

namespace Users.Library.Database;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        return new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>()
            .UseMySql(Configurations.ConnectionString, ServerVersion.AutoDetect(Configurations.ConnectionString))
            .Options);
    }
}
