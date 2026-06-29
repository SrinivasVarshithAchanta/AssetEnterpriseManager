using EnterpriseAssetManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Services;

public static class QueryablePagingExtensions
{
    // Applies Skip/Take on the database side and returns a single page of results
    // together with the total count, so list pages never load full tables into memory.
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        int total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}
