using System.ComponentModel.DataAnnotations;

namespace Users.Library.Models;

public class UpdateUserPassword
{
    [Required]
    public string? Password { get; set; }
}
