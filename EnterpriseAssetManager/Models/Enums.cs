namespace EnterpriseAssetManager.Models;

public enum AssetStatus
{
    Available = 0,
    Assigned = 1,
    UnderMaintenance = 2,
    Retired = 3
}

public enum AssetCondition
{
    New = 0,
    Good = 1,
    NeedsRepair = 2,
    Damaged = 3
}

public enum RequestPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum RequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Fulfilled = 3,
    Cancelled = 4
}

// Roles are stored as plain strings on the User entity and written into the
// authentication cookie as claims. Keeping them as constants avoids typos.
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
}
