using System.ComponentModel.DataAnnotations;

namespace EnterpriseAssetManager.Models;

public class AssetCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Name { get; set; }

    [MaxLength(250)]
    public string Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public ICollection<AssetRequest> Requests { get; set; } = new List<AssetRequest>();
}
