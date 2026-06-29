using System.ComponentModel.DataAnnotations;

namespace EnterpriseAssetManager.Models;

public class AuditLog
{
    public int Id { get; set; }

    // Nullable because some actions (for example a failed login) may not have a known user.
    public int? UserId { get; set; }
    public User User { get; set; }

    [Required]
    [MaxLength(80)]
    public string Action { get; set; }

    [Required]
    [MaxLength(80)]
    public string EntityName { get; set; }

    public int? EntityId { get; set; }

    [MaxLength(1000)]
    public string Details { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
