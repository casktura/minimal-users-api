using Microsoft.EntityFrameworkCore;

using Users.Library.Database.Models;

namespace Users.Library.Database;

public class UsersDbContext : DbContext
{
    internal DbSet<User> Users { get; set; }

    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }
}
