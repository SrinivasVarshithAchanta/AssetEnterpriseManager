using System.ComponentModel.DataAnnotations;
using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(80)]
    [Display(Name = "Category Name")]
    public string Name { get; set; }

    [MaxLength(250)]
    public string Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public class CategoryListViewModel
{
    public List<AssetCategory> Categories { get; set; } = new();
    public CategoryFormViewModel NewCategory { get; set; } = new();
}
