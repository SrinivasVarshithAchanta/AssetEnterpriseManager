using System.ComponentModel.DataAnnotations;

namespace EnterpriseAssetManager.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string FullName { get; set; }

    [Required]
    [MaxLength(160)]
    public string Email { get; set; }

    // Never stores the raw password. Filled by IPasswordHasher.
    [Required]
    public string PasswordHash { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = Roles.Employee;

    [MaxLength(80)]
    public string Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
