using Microsoft.EntityFrameworkCore;

using Users.Library.Database.Models;

namespace Users.Library.Database;

public class UsersDb : DbContext
{
    internal DbSet<User> Users { get; set; }

    public UsersDb(DbContextOptions options) : base(options)
    {
    }
}
