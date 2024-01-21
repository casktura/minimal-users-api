using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.Library.Database.Models;

internal class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Email { get; set; }

    [Required]
    public string? HashedPassword { get; set; }

    [Required]
    public string? Role { get; set; } = Constants.Role.User;

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastModifiedDate { get; set; } = DateTimeOffset.UtcNow;
}
