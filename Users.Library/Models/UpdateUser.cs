using System.ComponentModel.DataAnnotations;

namespace Users.Library.Models;

public class UpdateUser
{
    [Required]
    public string? Name { get; set; }

    public string? Email { get; set; }
}
