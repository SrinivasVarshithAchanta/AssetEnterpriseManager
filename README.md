# Enterprise Asset Manager (AssetOps)

A web-based **asset request and approval portal** for an organization, built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **SQL Server**. Employees request company assets (laptops, monitors, keyboards, ID cards, accessories), managers approve or reject those requests, and admins manage assets, categories, users, and audit logs.

This is a clean, interview-defendable project: a thin controller layer, a service layer that owns the business rules, EF Core for data access, cookie authentication with claims, role-based authorization, and xUnit tests for the core logic.

---

## Problem statement

Organizations hand out hardware and accessories constantly, and tracking *who asked for what*, *who approved it*, and *which physical asset was assigned* is usually done over email and spreadsheets. AssetOps replaces that with a single portal that:

- captures requests with a business reason and priority,
- routes them through a manager approval step,
- lets an admin fulfil an approved request by assigning a concrete available asset,
- keeps the asset inventory and its status (Available, Assigned, Under Maintenance, Retired) up to date,
- records an audit trail of important actions.

---

## Features

**Employee**
- Sign in and see a personal request summary (total, pending, approved, rejected).
- Create a new asset request (category, priority, business reason).
- View and cancel own pending requests; view request details.
- Cannot see other users' requests or approve anything.

**Manager**
- See the pending approvals queue.
- Filter requests by status, category, priority, and date range.
- Approve a request, or reject it **with a mandatory comment**.
- View asset availability.

**Admin**
- Dashboard with asset and request statistics plus recent activity.
- Manage assets (list, search, filter, add, edit, retire).
- Manage categories (add, edit, activate/deactivate).
- Manage users (add, edit, activate/deactivate, assign role).
- Fulfil approved requests by assigning an available asset.
- View the audit log.

---

## Roles and demo credentials

| Role     | Email                   | Password      | Lands on    |
|----------|-------------------------|---------------|-------------|
| Admin    | admin@assetops.com      | Admin@123     | Dashboard   |
| Manager  | manager@assetops.com    | Manager@123   | Approvals   |
| Employee | employee@assetops.com   | Employee@123  | My Requests |

The demo database is also seeded with **500 users** across departments (IT, HR, Finance, Engineering, Operations, Sales, Admin): 1 admin, 5 managers, and the rest employees. The demo employees use the password `Employee@123` and emails like `emp0001@assetops.com`.

---

## Tech stack

- **C# 12 / .NET 8**
- **ASP.NET Core MVC** (Razor views, not Blazor)
- **Entity Framework Core 8** (SQL Server provider)
- **SQL Server Express** (`.\SQLEXPRESS`, database `EnterpriseAssetManagerDb`)
- **Cookie authentication** using claims (no ASP.NET Identity)
- **Role-based authorization** via `[Authorize(Roles = "...")]`
- **Plain CSS** design system + **minimal vanilla JavaScript**
- **xUnit** + EF Core InMemory for service-layer tests

---

## Database design summary

| Entity         | Key fields | Notable rules |
|----------------|-----------|---------------|
| `User`         | Id, FullName, Email (unique), PasswordHash, Role, Department, IsActive | Email unique; FullName + Department indexed for search |
| `AssetCategory`| Id, Name (unique), Description, IsActive | |
| `Asset`        | Id, AssetTag (unique), Name, CategoryId, SerialNumber, Status, Condition, PurchaseDate, AssignedToUserId? | AssetTag unique; Name/Serial/Status indexed |
| `AssetRequest` | Id, RequestNumber (unique), RequestedByUserId, AssetCategoryId, AssetId?, BusinessReason, Priority, Status, ManagerComments?, RequestedAt, ReviewedAt?, ReviewedByUserId? | RequestNumber auto-generated + unique; Status/Priority/RequestedAt indexed |
| `AuditLog`     | Id, UserId?, Action, EntityName, EntityId?, Details, CreatedAt | CreatedAt indexed |

Relationships are configured with `OnDelete(DeleteBehavior.Restrict)` (and `SetNull` for audit logs) to avoid accidental cascading deletes.

**Key business rules (in the service layer):**
- AssetTag, Email, and RequestNumber are unique.
- Retired assets cannot be assigned.
- A request can only be fulfilled when it is Approved, using an Available asset.
- Fulfilling a request assigns the asset to the requester and sets the asset to Assigned.
- Rejecting a request requires a manager comment.
- Important actions are written to the audit log.

---

## How to run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server Express** running as `.\SQLEXPRESS` (service `MSSQL$SQLEXPRESS`)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### Connection string
Configured in `EnterpriseAssetManager/appsettings.json`:
```
Server=.\SQLEXPRESS;Database=EnterpriseAssetManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;
```

### Run
```powershell
# from the solution root
dotnet restore
dotnet build

# create the database from migrations (see migration commands below) the first time,
# then run the web app:
dotnet run --project EnterpriseAssetManager
```
The app applies migrations and seeds data automatically on startup, then prints the URL (for example `https://localhost:7xxx`). Open it and sign in with a demo account.

> First-run seeding inserts 500 users plus sample assets and requests, so the very first startup takes a few extra seconds.

---

## Migration commands

```powershell
# from the solution root
dotnet ef migrations add InitialCreate --project EnterpriseAssetManager
dotnet ef database update --project EnterpriseAssetManager
```

To reset the database during development:
```powershell
dotnet ef database drop --project EnterpriseAssetManager
dotnet ef database update --project EnterpriseAssetManager
```

---

## Test commands

```powershell
dotnet test
```

With coverage (coverlet is referenced by the test project):
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## SDLC practices used

- **Requirement breakdown** – features split by role (employee, manager, admin) and turned into controllers, services, and views.
- **Entity design** – five focused entities with clear relationships and indexes.
- **MVC architecture** – controllers handle HTTP, views render UI, models/entities hold data.
- **Service layer** – business rules live in services behind interfaces; controllers stay thin.
- **Validation** – DataAnnotations on view models, plus server-side rule checks in services.
- **Testing** – xUnit unit tests for password hashing, auth, requests, assets, users, and authorization policy.
- **Git workflow** – feature branches per area (see below) with meaningful commit checkpoints.
- **Manual test checklist** – see `TEST_PLAN.md`.

---

## Evidence and benchmarks

| Document | Purpose |
|----------|---------|
| `CV_CLAIM_EVIDENCE.md` | Maps each resume bullet to code + honest risk level |
| `PERFORMANCE_RESULTS.md` | Measured naive vs optimized query times |
| `COVERAGE_RESULTS.md` | Measured xUnit / coverlet coverage |
| `GIT_EVIDENCE.md` | Git/GitHub setup (repo not initialized by default) |
| `EnterpriseAssetManager.Benchmarks` | Console app to re-run performance comparison |

```cmd
dotnet run --project EnterpriseAssetManager.Benchmarks
dotnet test --collect:"XPlat Code Coverage"
```

---

## Suggested Git workflow

Branches:
- `main`
- `feature/authentication`
- `feature/asset-management`
- `feature/request-approval`
- `feature/testing`

Commit checkpoints:
1. Initial project setup
2. Add database models
3. Add authentication
4. Add asset management
5. Add request workflow
6. Add approval workflow
7. Add tests
8. UI polish and documentation

---

## Project structure

```
EnterpriseAssetManager.sln
EnterpriseAssetManager/            # ASP.NET Core MVC web app
  Controllers/                     # Account, Dashboard, Assets, AssetCategories,
                                   # Requests, Approvals, Users, AuditLogs, Api/
  Data/                            # ApplicationDbContext, DbSeeder
  Models/                          # User, AssetCategory, Asset, AssetRequest, AuditLog, Enums
  Services/                        # interfaces + implementations + helpers
  ViewModels/                      # form, list, and dashboard view models
  Views/                           # Razor views per controller + Shared layout/pager
  wwwroot/css/site.css             # design system
  wwwroot/js/site.js               # minimal JS
EnterpriseAssetManager.Tests/      # xUnit test project
README.md, INTERVIEW_NOTES.md, PERFORMANCE_NOTES.md,
ARCHITECTURE_NOTES.md, TEST_PLAN.md, TESTING_NOTES.md, AGILE_NOTES.md
```

---

## Interview explanation (quick version)

> AssetOps is an ASP.NET Core MVC app where employees request company assets, managers approve them, and admins assign the physical asset and manage inventory. I used cookie authentication with claims for three roles, EF Core with SQL Server for data, a service layer for the business rules, and xUnit for tests. The list pages use server-side filtering, pagination, indexes, and `AsNoTracking` so they stay fast even with hundreds of users and requests.

### Common interview questions and answers

**Why MVC and not Blazor / a SPA?**
The app is form-and-table heavy internal tooling. MVC with Razor keeps it simple, server-rendered, and easy to reason about, with minimal JavaScript.

**Why not ASP.NET Identity?**
Identity is powerful but heavy. For three roles and a simple login, cookie auth with claims plus a small PBKDF2 password hasher is easier to explain end to end and still secure.

**How do you keep controllers thin?**
Controllers only validate input, call a service, and pick a view or redirect. All rules (approval, fulfilment, uniqueness, hashing) live in services behind interfaces, which are injected via DI and unit tested.

**How do you store passwords?**
Never in plain text. `PasswordHasher` uses PBKDF2 (100k iterations, SHA-256, random per-user salt) and a constant-time comparison on verify.

**How is the approval workflow modeled?**
A request moves Pending → Approved/Rejected → Fulfilled (or Cancelled). Each transition is a service method that validates the current state, updates the entity, and writes an audit entry.

See `INTERVIEW_NOTES.md` for the architecture walkthrough and a claim-by-claim mapping to the code.
