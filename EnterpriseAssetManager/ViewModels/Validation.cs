using System.ComponentModel.DataAnnotations;

namespace EnterpriseAssetManager.ViewModels;

// Reusable rule used by the asset forms: a purchase date may be today or earlier,
// but never in the future.
public class NotFutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is null)
            return true; // let [Required] handle missing values

        if (value is DateTime date)
            return date.Date <= DateTime.Today;

        return false;
    }
}
