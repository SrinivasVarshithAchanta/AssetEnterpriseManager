using EnterpriseAssetManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers.Api;

// Small internal JSON API used by the request creation page to show how many
// assets are available in the chosen category. It demonstrates a clean,
// attribute routed API endpoint living alongside the MVC app.
[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetAvailabilityController : ControllerBase
{
    private readonly IAssetService _assets;

    public AssetAvailabilityController(IAssetService assets)
    {
        _assets = assets;
    }

    // GET /api/assets/availability?categoryId=3
    [HttpGet("availability")]
    public async Task<IActionResult> Availability([FromQuery] int categoryId)
    {
        int available = await _assets.CountAvailableByCategoryAsync(categoryId);
        return Ok(new { categoryId, available });
    }

    // GET /api/assets/search?term=laptop
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        var page = await _assets.GetPagedAsync(term, null, null, null, 1, 10);
        var items = page.Items.Select(a => new
        {
            a.Id,
            a.AssetTag,
            a.Name,
            Status = a.Status.ToString(),
            Category = a.Category != null ? a.Category.Name : null
        });

        return Ok(items);
    }
}
