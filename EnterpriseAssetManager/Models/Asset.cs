using System.ComponentModel.DataAnnotations;

namespace EnterpriseAssetManager.Models;

public class Asset
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public string AssetTag { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; }

    public int CategoryId { get; set; }
    public AssetCategory Category { get; set; }

    [Required]
    [MaxLength(80)]
    public string SerialNumber { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    public AssetCondition Condition { get; set; } = AssetCondition.New;

    public DateTime PurchaseDate { get; set; }

    // An asset can be unassigned (no holder), so this is nullable.
    public int? AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
