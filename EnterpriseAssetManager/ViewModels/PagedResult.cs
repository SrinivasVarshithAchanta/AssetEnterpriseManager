namespace EnterpriseAssetManager.ViewModels;

// Lightweight container returned by service list methods. It carries only the
// current page of rows plus the totals needed to render pagination controls.
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public int FirstItemOnPage => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
    public int LastItemOnPage => Math.Min(Page * PageSize, TotalCount);
}
