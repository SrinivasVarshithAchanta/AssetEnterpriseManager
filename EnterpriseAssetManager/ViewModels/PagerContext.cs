namespace EnterpriseAssetManager.ViewModels;

// Plain data passed to the shared _Pager partial. The partial preserves the
// current query string (filters) and only swaps the page number.
public class PagerContext
{
    public PagerContext(int page, int totalPages, int firstItem, int lastItem, int totalCount)
    {
        Page = page;
        TotalPages = totalPages;
        FirstItem = firstItem;
        LastItem = lastItem;
        TotalCount = totalCount;
    }

    public int Page { get; }
    public int TotalPages { get; }
    public int FirstItem { get; }
    public int LastItem { get; }
    public int TotalCount { get; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
