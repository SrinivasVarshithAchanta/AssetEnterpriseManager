using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Data;

// Seeds the database in two layers:
//   1. Core data  -> categories, the three demo login accounts, and a few assets (always present).
//   2. Demo data  -> 500 users across departments plus sample assets and requests
//                    so the dashboards, pagination, search and filters have realistic volume.
//
// All seeding is idempotent: it only inserts when the relevant table is empty or below target,
// so restarting the app does not duplicate rows.
public static class DbSeeder
{
    private static readonly string[] Departments =
        { "IT", "HR", "Finance", "Engineering", "Operations", "Sales", "Admin" };

    private static readonly string[] FirstNames =
        { "Aarav", "Vivaan", "Aditya", "Diya", "Ananya", "Ishaan", "Kabir", "Meera", "Riya", "Sai",
          "Nikhil", "Priya", "Rohan", "Sneha", "Arjun", "Kavya", "Varun", "Tara", "Dev", "Isha" };

    private static readonly string[] LastNames =
        { "Sharma", "Verma", "Reddy", "Nair", "Iyer", "Gupta", "Mehta", "Rao", "Das", "Bose",
          "Khan", "Patel", "Singh", "Menon", "Chopra", "Bansal", "Kapoor", "Joshi", "Pillai", "Shetty" };

    public static async Task SeedAsync(
        ApplicationDbContext db,
        IPasswordHasher hasher,
        IConfiguration configuration,
        ILogger logger)
    {
        await SeedCategoriesAsync(db, logger);
        await SeedCoreUsersAsync(db, hasher, logger);
        await SeedDemoUsersAsync(db, hasher, configuration, logger);
        await SeedAssetsAsync(db, logger);
        await SeedRequestsAsync(db, logger);
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.AssetCategories.AnyAsync())
            return;

        var categories = new[]
        {
            new AssetCategory { Name = "Laptop", Description = "Portable work computers." },
            new AssetCategory { Name = "Monitor", Description = "External display screens." },
            new AssetCategory { Name = "Keyboard", Description = "Wired and wireless keyboards." },
            new AssetCategory { Name = "Mouse", Description = "Pointing devices." },
            new AssetCategory { Name = "ID Card", Description = "Employee access cards." },
            new AssetCategory { Name = "Docking Station", Description = "Port replicators for laptops." },
            new AssetCategory { Name = "Headset", Description = "Audio headsets for calls." }
        };

        db.AssetCategories.AddRange(categories);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} asset categories.", categories.Length);
    }

    private static async Task SeedCoreUsersAsync(ApplicationDbContext db, IPasswordHasher hasher, ILogger logger)
    {
        await EnsureUserAsync(db, hasher, "admin@assetops.com", "Admin@123", "System Administrator", Roles.Admin, "Admin");
        await EnsureUserAsync(db, hasher, "manager@assetops.com", "Manager@123", "Default Manager", Roles.Manager, "Operations");
        await EnsureUserAsync(db, hasher, "employee@assetops.com", "Employee@123", "Default Employee", Roles.Employee, "Engineering");
        await db.SaveChangesAsync();
        logger.LogInformation("Core login accounts ensured.");
    }

    private static async Task EnsureUserAsync(
        ApplicationDbContext db, IPasswordHasher hasher,
        string email, string password, string fullName, string role, string department)
    {
        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        db.Users.Add(new User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            FullName = fullName,
            Role = role,
            Department = department,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static async Task SeedDemoUsersAsync(
        ApplicationDbContext db, IPasswordHasher hasher, IConfiguration configuration, ILogger logger)
    {
        bool enabled = configuration.GetValue("SeedDemoData:Enabled", true);
        int target = configuration.GetValue("SeedDemoData:TotalUsers", 500);
        if (!enabled)
            return;

        int current = await db.Users.CountAsync();
        if (current >= target)
            return;

        // Reuse one hash per password so seeding 500 users stays fast.
        string managerHash = hasher.Hash("Manager@123");
        string employeeHash = hasher.Hash("Employee@123");

        var random = new Random(20260629);
        var newUsers = new List<User>();

        // Top up managers so there are 5 in total (one already exists from core seed).
        int managerCount = await db.Users.CountAsync(u => u.Role == Roles.Manager);
        for (int i = managerCount + 1; i <= 5; i++)
        {
            newUsers.Add(new User
            {
                Email = $"manager{i}@assetops.com",
                PasswordHash = managerHash,
                FullName = $"{Pick(FirstNames, random)} {Pick(LastNames, random)}",
                Role = Roles.Manager,
                Department = Pick(Departments, random),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 400))
            });
        }

        // Fill the remainder with employees up to the target head count.
        int employeesToAdd = target - current - newUsers.Count;
        for (int i = 1; i <= employeesToAdd; i++)
        {
            newUsers.Add(new User
            {
                Email = $"emp{i:0000}@assetops.com",
                PasswordHash = employeeHash,
                FullName = $"{Pick(FirstNames, random)} {Pick(LastNames, random)}",
                Role = Roles.Employee,
                Department = Pick(Departments, random),
                IsActive = random.Next(0, 100) < 95, // a few inactive accounts for realism
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 500))
            });
        }

        db.Users.AddRange(newUsers);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} demo users (target {Target}).", newUsers.Count, target);
    }

    private static async Task SeedAssetsAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Assets.AnyAsync())
            return;

        var categories = await db.AssetCategories.ToListAsync();
        if (categories.Count == 0)
            return;

        var random = new Random(777);
        var assets = new List<Asset>();
        int tagNumber = 1000;

        for (int i = 1; i <= 140; i++)
        {
            var category = categories[random.Next(categories.Count)];
            var status = WeightedStatus(random);

            assets.Add(new Asset
            {
                AssetTag = $"AST-{tagNumber++}",
                Name = $"{category.Name} Unit {i:000}",
                CategoryId = category.Id,
                SerialNumber = $"SN-{random.Next(100000, 999999)}-{i:000}",
                Status = status,
                Condition = (AssetCondition)random.Next(0, 4),
                PurchaseDate = DateTime.Today.AddDays(-random.Next(60, 1200)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 600))
            });
        }

        db.Assets.AddRange(assets);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} assets.", assets.Count);
    }

    private static async Task SeedRequestsAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.AssetRequests.AnyAsync())
            return;

        var employeeIds = await db.Users.Where(u => u.Role == Roles.Employee).Select(u => u.Id).ToListAsync();
        var managerId = await db.Users.Where(u => u.Role == Roles.Manager).Select(u => u.Id).FirstOrDefaultAsync();
        var categoryIds = await db.AssetCategories.Select(c => c.Id).ToListAsync();
        if (employeeIds.Count == 0 || categoryIds.Count == 0)
            return;

        var reasons = new[]
        {
            "Current device is slow and affects daily productivity.",
            "Need a second monitor for development and testing work.",
            "Replacement required after hardware failure last week.",
            "New joiner setup for the engineering onboarding process.",
            "Existing accessory is damaged and needs replacement soon.",
            "Required for remote work and customer demo sessions."
        };

        var random = new Random(555);
        var requests = new List<AssetRequest>();
        int sequence = 1;

        for (int i = 0; i < 260; i++)
        {
            var status = WeightedRequestStatus(random);
            var requestedAt = DateTime.UtcNow.AddDays(-random.Next(0, 180));

            var request = new AssetRequest
            {
                RequestNumber = $"REQ-2026-{sequence++:000000}",
                RequestedByUserId = employeeIds[random.Next(employeeIds.Count)],
                AssetCategoryId = categoryIds[random.Next(categoryIds.Count)],
                BusinessReason = reasons[random.Next(reasons.Length)],
                Priority = (RequestPriority)random.Next(0, 3),
                Status = status,
                RequestedAt = requestedAt
            };

            if (status is RequestStatus.Approved or RequestStatus.Rejected or RequestStatus.Fulfilled)
            {
                request.ReviewedByUserId = managerId == 0 ? null : managerId;
                request.ReviewedAt = requestedAt.AddDays(random.Next(1, 6));
                if (status == RequestStatus.Rejected)
                    request.ManagerComments = "Not approved due to current budget and stock limits.";
            }

            requests.Add(request);
        }

        db.AssetRequests.AddRange(requests);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} asset requests.", requests.Count);
    }

    private static AssetStatus WeightedStatus(Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 55) return AssetStatus.Available;
        if (roll < 80) return AssetStatus.Assigned;
        if (roll < 92) return AssetStatus.UnderMaintenance;
        return AssetStatus.Retired;
    }

    private static RequestStatus WeightedRequestStatus(Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 30) return RequestStatus.Pending;
        if (roll < 55) return RequestStatus.Approved;
        if (roll < 72) return RequestStatus.Rejected;
        if (roll < 90) return RequestStatus.Fulfilled;
        return RequestStatus.Cancelled;
    }

    private static string Pick(string[] values, Random random) => values[random.Next(values.Length)];
}
