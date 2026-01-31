using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("users")]
public class User
{
    [Key]
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
