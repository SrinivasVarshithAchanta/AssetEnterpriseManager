# Interview Notes — Enterprise Asset Manager (AssetOps)

Honest preparation guide. Cross-reference: `CV_CLAIM_EVIDENCE.md`, `PERFORMANCE_RESULTS.md`, `COVERAGE_RESULTS.md`.

---

## 30-second explanation

> AssetOps is an ASP.NET Core MVC app I built for company asset requests — employees request laptops or monitors, managers approve or reject, and admins assign the actual asset from inventory. It uses EF Core with SQL Server Express, cookie authentication with three roles, a service layer for business rules, and xUnit tests on the core logic.

---

## 1-minute explanation

> The problem was tracking who requested what equipment and which physical unit they received — usually done in email or spreadsheets. I built AssetOps as a web portal: employees submit requests with a business reason, managers approve or reject with comments, and admins fulfil approved requests by picking an available asset, which updates both the request and asset status. Technically it's ASP.NET Core MVC on .NET 8, EF Core to SQL Server Express, with thin controllers and injectable services behind interfaces. List pages use server-side filtering and pagination with indexes and `AsNoTracking`. I tested with 500 seeded demo users, documented query benchmarks, and wrote xUnit tests for hashing, auth, and the approval workflow.

---

## 2-minute explanation

> AssetOps solves internal equipment tracking. Employees log in, pick a category like Laptop, set priority, and explain why they need it. That creates a Pending request with an auto-generated number. Managers see a queue, filter by priority or date, and approve or reject — rejection requires a comment. Admins manage the asset catalog and, when a request is Approved, assign a specific Available asset; the asset moves to Assigned and the request to Fulfilled. Everything important is audit-logged.
>
> Architecturally: browser hits MVC controllers, controllers call services like `IRequestService`, services use `ApplicationDbContext` and SQL Server Express. Authentication is cookie-based with role claims — no ASP.NET Identity, so it's easier to explain: `AuthService` validates credentials, `PasswordHasher` uses PBKDF2, and `[Authorize(Roles=...)]` gates controllers.
>
> For scale I seeded 500 demo users and optimized list queries — benchmarks show ~34% faster overall vs loading full tables into memory. I have xUnit tests on the service layer and measured coverage with coverlet. I describe metrics honestly: demo users, not production, and coverage is strong on services but not 85% across the whole app unless I expand tests.

---

## Technical architecture

```
Browser (Razor + CSS + minimal JS)
    → MVC Controllers (thin)
        → Services (IAssetService, IRequestService, IUserService, IAuthService, IAuditService)
            → ApplicationDbContext (EF Core)
                → SQL Server Express (.\SQLEXPRESS, EnterpriseAssetManagerDb)
```

- **ViewModels** for forms; **entities** for persistence.
- **Internal API:** `Api/AssetAvailabilityController` for live category availability on request form.
- **Startup:** `Program.cs` registers DI, cookie auth, runs `Migrate()` + `DbSeeder`.

---

## Database schema (5 tables)

| Table | Purpose |
|-------|---------|
| `Users` | Login, role, department; unique email |
| `AssetCategories` | Laptop, Monitor, etc. |
| `Assets` | Physical items; status Available/Assigned/Maintenance/Retired |
| `AssetRequests` | Workflow; unique RequestNumber |
| `AuditLogs` | Who did what, when |

Relationships use `Restrict` delete to avoid accidental cascades.

---

## Role-based access

| Role | Can do |
|------|--------|
| **Employee** | Own requests only; create, cancel pending, view dashboard summary |
| **Manager** | Approvals queue; approve/reject; view assets |
| **Admin** | Full inventory, users, categories, fulfil requests, audit logs |

Enforced by `[Authorize(Roles = "...")]` on controllers. `AuthorizationPolicyTests` verifies Employees cannot reach approvals.

Demo logins: `admin@assetops.com` / `Admin@123`, `manager@assetops.com` / `Manager@123`, `employee@assetops.com` / `Employee@123`.

---

## Approval workflow

```
Pending → (Manager Approve) → Approved → (Admin Fulfil + assign asset) → Fulfilled
Pending → (Manager Reject + comment) → Rejected
Pending → (Employee Cancel) → Cancelled
```

Rules in `RequestService`: reject needs comment; fulfil only on Approved; cannot assign Retired asset.

---

## Performance optimization

- Production: `IQueryable` + `Where` + `Skip`/`Take` + `AsNoTracking` + indexes.
- Evidence: `EnterpriseAssetManager.Benchmarks` → `PERFORMANCE_RESULTS.md`.
- Measured 2026-06-29: **33.9%** overall vs naive in-memory approach on seeded data (slightly below 35% claim — use careful wording).

---

## Testing

- **34 xUnit tests** — services + authorization attributes.
- **coverlet:** ~14.6% whole assembly, ~69.7% services average — see `COVERAGE_RESULTS.md`.
- Manual: `TEST_PLAN.md` (22 cases).

---

## What I personally learned

- How to keep controllers thin and put business rules in testable services.
- Cookie authentication with claims without pulling in full Identity.
- Why loading entire tables into memory breaks down as data grows — and how EF Core translates `IQueryable` to SQL.
- Writing honest documentation for resume claims (benchmarks and coverage you can show in an interview).
- EF Core migrations and indexing for real SQL Server Express.

---

## What I would improve next

- Asset return/check-in flow.
- Integration tests against SQL Server (not just InMemory).
- More service tests to raise measured coverage.
- Email notifications on status changes.
- Initialize Git repo and push to GitHub with real commit history.

---

## How this project matches my resume

See `CV_CLAIM_EVIDENCE.md` for the full table with risk levels.

| Claim | Evidence |
|-------|----------|
| ASP.NET Core + SQL Server | `Program.cs`, migrations, `appsettings.json` → `.\SQLEXPRESS` |
| 500 users | `DbSeeder.cs` — **seeded demo**, not live production |
| OOP services | `Services/` interfaces + DI |
| 35% faster | `PERFORMANCE_RESULTS.md` — **33.9% measured** |
| xUnit | `EnterpriseAssetManager.Tests` — 34 tests |
| 85% coverage | **Not supported** at assembly level; ~70% services avg — see `COVERAGE_RESULTS.md` |
| GitHub | `.gitignore` + `GIT_EVIDENCE.md` — **repo not initialized yet** |
| Agile | `AGILE_NOTES.md` — Agile-**style** planning |

---

## Common interview Q&A

**Why MVC not React?**  
Internal admin tool — forms and tables fit server-rendered MVC; less complexity.

**Why not Identity?**  
Three roles, simple login — custom cookie + PBKDF2 is enough and easier to explain.

**Hardest part?**  
Fulfilment: update request + asset atomically with correct status guards — `RequestService.FulfillAsync` + unit tests.

**How do you know it's fast?**  
Benchmark console app comparing naive `ToListAsync` vs optimized queries; indexes on searched columns.
