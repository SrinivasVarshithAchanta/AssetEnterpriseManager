using System.ComponentModel.DataAnnotations;
using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.ViewModels;

public class AssetCreateViewModel
{
    [Required(ErrorMessage = "Asset tag is required.")]
    [MaxLength(40)]
    [Display(Name = "Asset Tag")]
    public string AssetTag { get; set; }

    [Required(ErrorMessage = "Asset name is required.")]
    [MaxLength(120)]
    [Display(Name = "Asset Name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Serial number is required.")]
    [MaxLength(80)]
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; }

    [Required]
    public AssetStatus Status { get; set; } = AssetStatus.Available;

    [Required]
    public AssetCondition Condition { get; set; } = AssetCondition.New;

    [Required(ErrorMessage = "Purchase date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Purchase Date")]
    [NotFutureDate(ErrorMessage = "Purchase date cannot be in the future.")]
    public DateTime PurchaseDate { get; set; } = DateTime.Today;

    public List<AssetCategory> Categories { get; set; } = new();
}

public class AssetEditViewModel : AssetCreateViewModel
{
    public int Id { get; set; }
}

public class AssetListViewModel
{
    public PagedResult<Asset> Results { get; set; } = new();
    public string Search { get; set; }
    public int? CategoryId { get; set; }
    public AssetStatus? Status { get; set; }
    public AssetCondition? Condition { get; set; }
    public List<AssetCategory> Categories { get; set; } = new();
}
