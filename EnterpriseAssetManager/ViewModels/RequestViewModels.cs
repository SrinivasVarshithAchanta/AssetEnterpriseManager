using System.ComponentModel.DataAnnotations;
using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.ViewModels;

public class AssetRequestCreateViewModel
{
    [Required(ErrorMessage = "Asset category is required.")]
    [Display(Name = "Asset Category")]
    public int AssetCategoryId { get; set; }

    [Required(ErrorMessage = "Business reason is required.")]
    [MinLength(10, ErrorMessage = "Business reason must be at least 10 characters.")]
    [MaxLength(500)]
    [Display(Name = "Business Reason")]
    public string BusinessReason { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;

    public List<AssetCategory> Categories { get; set; } = new();
}

public class MyRequestListViewModel
{
    public PagedResult<AssetRequest> Results { get; set; } = new();
    public RequestStatus? Status { get; set; }
}

public class ApprovalListViewModel
{
    public PagedResult<AssetRequest> Results { get; set; } = new();
    public RequestStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public RequestPriority? Priority { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    public List<AssetCategory> Categories { get; set; } = new();
}

// Used on the approval details page for both approve and reject actions.
public class RequestApprovalViewModel
{
    public int RequestId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Manager Comments")]
    public string Comments { get; set; }

    public AssetRequest Request { get; set; }
}

// Used by the admin to fulfil an approved request by picking an available asset.
public class FulfillRequestViewModel
{
    public int RequestId { get; set; }

    [Required(ErrorMessage = "Select an available asset to assign.")]
    [Display(Name = "Asset to assign")]
    public int AssetId { get; set; }

    public AssetRequest Request { get; set; }
    public List<Asset> AvailableAssets { get; set; } = new();
}
