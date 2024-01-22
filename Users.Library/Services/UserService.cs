using Microsoft.EntityFrameworkCore;

using Users.Library.Database;
using Users.Library.Extensions;
using Users.Library.Models;
using Users.Library.Securities;

namespace Users.Library.Services;

public class UserService
{
    private readonly UsersDb _usersDb;

    public UserService(UsersDb usersDb)
    {
        _usersDb = usersDb;
    }

    public async Task<bool> VerifyUserPasswordAsync(Login login)
    {
        var user = await _usersDb.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == login.Email && !u.IsDeleted);

        if (user == null)
        {
            return false;
        }
        else
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.VerifyHashedPassword(user.HashedPassword, login.Password);
        }
    }

    public async Task<IList<User>> FilterUsersAsync(int[]? userIds = null, string? search = null)
    {
        return await _usersDb
            .Users
            .Where(u =>
                !u.IsDeleted
                && (userIds == null || userIds.Length == 0 || userIds.Contains(u.Id))
                && (string.IsNullOrWhiteSpace(search) || u.Name.Contains(search) || u.Email.Contains(search)))
            .Select(u => u.ToDomainModel())
            .ToListAsync();
    }

    public async Task<User> CreateUserAsync(CreateUser createUser)
    {
        if (await _usersDb.Users.AnyAsync(u => u.Email == createUser.Email))
        {
            throw new InvalidOperationException("Email is already used!");
        }

        var passwordHasher = new PasswordHasher();
        var user = new Database.Models.User
        {
            Name = createUser.Name,
            Email = createUser.Email,
            HashedPassword = passwordHasher.HashPassword(createUser.Password)
        };

        _ = _usersDb.Users.Add(user);
        _ = await _usersDb.SaveChangesAsync();

        return user.ToDomainModel();
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUser updateUser)
    {
        var user = await _usersDb.Users.SingleOrDefaultAsync(u => u.Id == userId && !u.IsDeleted) ?? throw new InvalidOperationException("User not found!");
        user.Name = updateUser.Name;
        user.Email = updateUser.Email;
        user.LastModifiedDate = DateTime.UtcNow;
        _ = await _usersDb.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserPasswordAsync(int userId, UpdateUserPassword updateUserPassword)
    {
        var user = await _usersDb.Users.SingleOrDefaultAsync(u => u.Id == userId && !u.IsDeleted) ?? throw new InvalidOperationException("User not found!");
        var passwordHasher = new PasswordHasher();
        user.HashedPassword = passwordHasher.HashPassword(updateUserPassword.Password);
        user.LastModifiedDate = DateTime.UtcNow;
        _ = await _usersDb.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _usersDb.Users.SingleOrDefaultAsync(u => u.Id == userId && !u.IsDeleted) ?? throw new InvalidOperationException("User not found!");
        user.IsDeleted = true;
        user.LastModifiedDate = DateTime.UtcNow;
        _ = await _usersDb.SaveChangesAsync();

        return true;
    }
}
