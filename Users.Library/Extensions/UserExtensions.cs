using Users.Library.Models;

namespace Users.Library.Extensions;

internal static class UserExtensions
{
    public static User ToDomainModel(this Database.Models.User user)
    {
        return new User
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}
