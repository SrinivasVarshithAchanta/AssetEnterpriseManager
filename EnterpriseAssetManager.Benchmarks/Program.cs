using System.Diagnostics;
using System.Text;
using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Compares naive in-memory filtering vs optimized IQueryable + AsNoTracking + paging
// against the real SQL Server Express database used by the web app.
const int WarmupRuns = 2;
const int MeasuredRuns = 8;
const int Page = 1;
const int PageSize = 10;

var root = FindSolutionRoot();
var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(root, "EnterpriseAssetManager"))
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("ERROR: DefaultConnection not found in appsettings.json");
    return 1;
}

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var db = new ApplicationDbContext(options);

if (!await db.Database.CanConnectAsync())
{
    Console.WriteLine("ERROR: Cannot connect to SQL Server. Ensure SQLEXPRESS is running and the app has been seeded.");
    return 1;
}

int userCount = await db.Users.CountAsync();
int assetCount = await db.Assets.CountAsync();
int requestCount = await db.AssetRequests.CountAsync();

Console.WriteLine("Enterprise Asset Manager — list/search benchmark");
Console.WriteLine($"Database: SQL Server Express (connection from appsettings.json)");
Console.WriteLine($"Counts: Users={userCount}, Assets={assetCount}, Requests={requestCount}");
Console.WriteLine($"Warmup={WarmupRuns}, Measured runs={MeasuredRuns}, Page={Page}, PageSize={PageSize}");
Console.WriteLine();

var results = new List<BenchmarkResult>
{
    await RunScenarioAsync("Asset search (tag/name/serial)", "AST",
        () => NaiveAssetSearchAsync(db, "AST", Page, PageSize),
        () => OptimizedAssetSearchAsync(db, "AST", Page, PageSize)),

    await RunScenarioAsync("User search (name/email)", "emp",
        () => NaiveUserSearchAsync(db, "emp", null, Page, PageSize),
        () => OptimizedUserSearchAsync(db, "emp", null, Page, PageSize)),

    await RunScenarioAsync("Request filter (status/priority)", null,
        () => NaiveRequestFilterAsync(db, RequestStatus.Pending, RequestPriority.Medium, Page, PageSize),
        () => OptimizedRequestFilterAsync(db, RequestStatus.Pending, RequestPriority.Medium, Page, PageSize)),

    await RunScenarioAsync("Paginated asset list (no search)", null,
        () => NaiveAssetSearchAsync(db, null, Page, PageSize),
        () => OptimizedAssetSearchAsync(db, null, Page, PageSize))
};

var reportPath = Path.Combine(root, "PERFORMANCE_RESULTS.md");
await File.WriteAllTextAsync(reportPath, BuildMarkdownReport(results, userCount, assetCount, requestCount), Encoding.UTF8);

Console.WriteLine("Scenario                              Naive(ms)  Optimized(ms)  Improvement");
Console.WriteLine(new string('-', 78));
foreach (var r in results)
{
    Console.WriteLine($"{r.Name,-38} {r.NaiveAverageMs,9:F1} {r.OptimizedAverageMs,14:F1} {r.ImprovementPercent,11:F1}%");
}
Console.WriteLine();
Console.WriteLine($"Report written to: {reportPath}");
return 0;

static string FindSolutionRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "EnterpriseAssetManager.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }
    throw new InvalidOperationException("Could not locate solution root.");
}

static async Task<BenchmarkResult> RunScenarioAsync(
    string name, string searchHint,
    Func<Task> naive, Func<Task> optimized)
{
    for (int i = 0; i < WarmupRuns; i++)
    {
        await naive();
        await optimized();
    }

    var naiveTimes = new List<double>();
    var optimizedTimes = new List<double>();

    for (int i = 0; i < MeasuredRuns; i++)
    {
        naiveTimes.Add(await TimeAsync(naive));
        optimizedTimes.Add(await TimeAsync(optimized));
    }

    double naiveAvg = naiveTimes.Average();
    double optAvg = optimizedTimes.Average();
    double improvement = naiveAvg <= 0 ? 0 : ((naiveAvg - optAvg) / naiveAvg) * 100.0;

    return new BenchmarkResult(name, searchHint, naiveAvg, optAvg, improvement);
}

static async Task<double> TimeAsync(Func<Task> action)
{
    var sw = Stopwatch.StartNew();
    await action();
    sw.Stop();
    return sw.Elapsed.TotalMilliseconds;
}

// ---------- Naive (anti-pattern): load all rows, filter in memory ----------

static async Task NaiveAssetSearchAsync(ApplicationDbContext db, string search, int page, int pageSize)
{
    var all = await db.Assets.Include(a => a.Category).ToListAsync();
    IEnumerable<Asset> filtered = all;
    if (!string.IsNullOrWhiteSpace(search))
    {
        filtered = all.Where(a =>
            a.AssetTag.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            a.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            a.SerialNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
    _ = filtered.OrderBy(a => a.AssetTag).Skip((page - 1) * pageSize).Take(pageSize).ToList();
}

static async Task NaiveUserSearchAsync(ApplicationDbContext db, string search, string role, int page, int pageSize)
{
    var all = await db.Users.ToListAsync();
    IEnumerable<User> filtered = all;
    if (!string.IsNullOrWhiteSpace(search))
    {
        filtered = all.Where(u =>
            u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
    if (!string.IsNullOrWhiteSpace(role))
        filtered = filtered.Where(u => u.Role == role);
    _ = filtered.OrderBy(u => u.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToList();
}

static async Task NaiveRequestFilterAsync(
    ApplicationDbContext db, RequestStatus status, RequestPriority priority, int page, int pageSize)
{
    var all = await db.AssetRequests
        .Include(r => r.RequestedByUser)
        .Include(r => r.AssetCategory)
        .ToListAsync();
    var filtered = all
        .Where(r => r.Status == status && r.Priority == priority)
        .OrderByDescending(r => r.RequestedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    _ = filtered.Count;
}

// ---------- Optimized: IQueryable + AsNoTracking + DB-side paging ----------

static async Task OptimizedAssetSearchAsync(ApplicationDbContext db, string search, int page, int pageSize)
{
    IQueryable<Asset> query = db.Assets.AsNoTracking().Include(a => a.Category);
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(a =>
            a.AssetTag.Contains(search) ||
            a.Name.Contains(search) ||
            a.SerialNumber.Contains(search));
    }
    await query.OrderBy(a => a.AssetTag).ToPagedResultAsync(page, pageSize);
}

static async Task OptimizedUserSearchAsync(ApplicationDbContext db, string search, string role, int page, int pageSize)
{
    IQueryable<User> query = db.Users.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
    if (!string.IsNullOrWhiteSpace(role))
        query = query.Where(u => u.Role == role);
    await query.OrderBy(u => u.FullName).ToPagedResultAsync(page, pageSize);
}

static async Task OptimizedRequestFilterAsync(
    ApplicationDbContext db, RequestStatus status, RequestPriority priority, int page, int pageSize)
{
    IQueryable<AssetRequest> query = db.AssetRequests
        .AsNoTracking()
        .Include(r => r.RequestedByUser)
        .Include(r => r.AssetCategory)
        .Where(r => r.Status == status && r.Priority == priority);
    await query.OrderByDescending(r => r.RequestedAt).ToPagedResultAsync(page, pageSize);
}

static string BuildMarkdownReport(
    List<BenchmarkResult> results, int users, int assets, int requests)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Performance Benchmark Results");
    sb.AppendLine();
    sb.AppendLine("> Auto-generated by `EnterpriseAssetManager.Benchmarks`. Re-run after seed data changes.");
    sb.AppendLine();
    sb.AppendLine($"**Run date (UTC):** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine("**Database:** SQL Server Express `\\.\\SQLEXPRESS`");
    sb.AppendLine("**Database name:** `EnterpriseAssetManagerDb`");
    sb.AppendLine($"**Seed data detected:** Users={users}, Assets={assets}, AssetRequests={requests}");
    sb.AppendLine("**Method:** `Stopwatch` over in-process EF Core calls (warmup + measured runs)");
    sb.AppendLine();
    sb.AppendLine("## Formula");
    sb.AppendLine("```");
    sb.AppendLine("Improvement % = ((NaiveTime - OptimizedTime) / NaiveTime) * 100");
    sb.AppendLine("```");
    sb.AppendLine();
    sb.AppendLine("| Scenario | Naive avg (ms) | Optimized avg (ms) | Improvement % |");
    sb.AppendLine("|----------|----------------|--------------------|---------------|");

    double totalNaive = 0, totalOpt = 0;
    foreach (var r in results)
    {
        sb.AppendLine($"| {r.Name} | {r.NaiveAverageMs:F1} | {r.OptimizedAverageMs:F1} | {r.ImprovementPercent:F1} |");
        totalNaive += r.NaiveAverageMs;
        totalOpt += r.OptimizedAverageMs;
    }

    double overall = totalNaive <= 0 ? 0 : ((totalNaive - totalOpt) / totalNaive) * 100.0;
    sb.AppendLine($"| **Overall (sum of scenarios)** | **{totalNaive:F1}** | **{totalOpt:F1}** | **{overall:F1}** |");
    sb.AppendLine();
    sb.AppendLine("## Interpretation");
    sb.AppendLine("- **Naive approach:** `ToListAsync()` on full tables, then filter/page in C#.");
    sb.AppendLine("- **Optimized approach:** `IQueryable` + `Where` + `OrderBy` + `Skip`/`Take` + `AsNoTracking()` (same pattern as production services).");
    sb.AppendLine();

    if (overall >= 35.0)
    {
        sb.AppendLine($"The measured overall improvement is **{overall:F1}%**, which supports the resume wording about ~35% faster list/search operations on this dataset.");
    }
    else
    {
        sb.AppendLine($"The measured overall improvement is **{overall:F1}%**, which is **below the 35% resume claim** on this machine/dataset.");
        sb.AppendLine();
        sb.AppendLine("**Safer interview wording:**");
        sb.AppendLine("> Optimized list and search pages using server-side filtering, pagination, database indexes, and `AsNoTracking()` instead of loading full tables into memory.");
        sb.AppendLine();
        sb.AppendLine("The optimization is still real — the gap may grow with larger tables or concurrent load — but do not state \"35%\" unless you re-run this benchmark and the result supports it.");
    }

    sb.AppendLine();
    sb.AppendLine("## How to re-run");
    sb.AppendLine("```cmd");
    sb.AppendLine("cd \"C:\\Users\\s_var\\OneDrive\\Desktop\\Personal projects\\EnterpriseAssetManager\"");
    sb.AppendLine("dotnet run --project EnterpriseAssetManager.Benchmarks");
    sb.AppendLine("```");
    return sb.ToString();
}

record BenchmarkResult(string Name, string SearchHint, double NaiveAverageMs, double OptimizedAverageMs, double ImprovementPercent);
