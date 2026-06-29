namespace EnterpriseAssetManager.ViewModels;

public class AssetStats
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Assigned { get; set; }
    public int UnderMaintenance { get; set; }
    public int Retired { get; set; }
}

public class RequestStats
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Fulfilled { get; set; }
    public int Cancelled { get; set; }
}
