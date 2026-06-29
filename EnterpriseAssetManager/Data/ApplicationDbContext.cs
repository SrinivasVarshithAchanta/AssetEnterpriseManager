using EnterpriseAssetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetRequest> AssetRequests => Set<AssetRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- User ----------
        modelBuilder.Entity<User>(entity =>
        {
            // Email must be unique across the organization.
            entity.HasIndex(u => u.Email).IsUnique();
            // FullName and Department are searched/filtered on the Users list page.
            entity.HasIndex(u => u.FullName);
            entity.HasIndex(u => u.Department);
        });

        // ---------- AssetCategory ----------
        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // ---------- Asset ----------
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.AssetTag).IsUnique();
            entity.HasIndex(a => a.Name);
            entity.HasIndex(a => a.SerialNumber);
            entity.HasIndex(a => a.Status);

            entity.HasOne(a => a.Category)
                  .WithMany(c => c.Assets)
                  .HasForeignKey(a => a.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.AssignedToUser)
                  .WithMany()
                  .HasForeignKey(a => a.AssignedToUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- AssetRequest ----------
        modelBuilder.Entity<AssetRequest>(entity =>
        {
            entity.HasIndex(r => r.RequestNumber).IsUnique();
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.Priority);
            entity.HasIndex(r => r.RequestedAt);

            entity.HasOne(r => r.RequestedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.RequestedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.AssetCategory)
                  .WithMany(c => c.Requests)
                  .HasForeignKey(r => r.AssetCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Asset)
                  .WithMany()
                  .HasForeignKey(r => r.AssetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReviewedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- AuditLog ----------
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.CreatedAt);

            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
