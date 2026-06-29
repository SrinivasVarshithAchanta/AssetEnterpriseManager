using System.ComponentModel.DataAnnotations;
using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.ViewModels;

public class UserCreateViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(120)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(160)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = Roles.Employee;

    [MaxLength(80)]
    public string Department { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(120)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    // Email is shown read only on the edit form to keep the unique key stable.
    public string Email { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; }

    [MaxLength(80)]
    public string Department { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}

public class UserListViewModel
{
    public PagedResult<User> Results { get; set; } = new();
    public string Search { get; set; }
    public string Role { get; set; }
    public bool? IsActive { get; set; }
}
