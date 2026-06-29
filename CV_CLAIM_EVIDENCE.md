# CV Claim Evidence Map

Honest mapping between resume bullets and project proof. Use **Interview-safe wording** in the right column when the risk is Medium or Weak.

| # | Resume claim | Project evidence | Files / pages | Interview-safe wording | Risk |
|---|--------------|------------------|---------------|------------------------|------|
| 1 | **C#, ASP.NET Core, SQL Server, HTML, CSS, JavaScript, Git** | Full stack MVC app on .NET 8; EF Core → SQL Server Express; custom CSS in `wwwroot/css/site.css`; minimal JS in `site.js`; `.gitignore` + `GIT_EVIDENCE.md` | `Program.cs`, `appsettings.json`, `wwwroot/`, `.gitignore` | “Built with ASP.NET Core MVC, EF Core, SQL Server Express, plain CSS/JS, and Git-ready structure.” | **Strong** (Git: Medium until repo initialized) |
| 2 | **Equipment request/approval portal** | Employee requests → manager approve/reject → admin fulfil with asset assignment | `RequestsController`, `ApprovalsController`, `RequestService`, views under `Views/Requests`, `Views/Approvals` | “Employees raise requests; managers approve or reject with comments; admins assign a physical asset when fulfilling.” | **Strong** |
| 3 | **500 users** | **Seeded demo users**, not production. `DbSeeder` targets 500 users; pagination on Users list | `Data/DbSeeder.cs`, `UsersController`, `Views/Users/Index.cshtml`, `appsettings.json` `SeedDemoData` | “Tested with **500 seeded demo users** across departments to simulate org scale; pagination and search keep lists usable.” | **Medium** — say “seeded/demo”, not “live 500 users” |
| 4 | **Agile / team collaboration** | Sprint-style plan, user stories, acceptance criteria — planning structure, not proof of a corporate team | `AGILE_NOTES.md` | “Used **Agile-style planning**: sprint-like phases, user stories, and acceptance criteria while building the project.” | **Medium** — see `AGILE_NOTES.md` |
| 5 | **OOP modular backend services** | Interfaces + implementations; DI in `Program.cs`; thin controllers | `Services/*.cs`, `ARCHITECTURE_NOTES.md` | “Business rules live in injectable service interfaces — assets, requests, users, auth, audit.” | **Strong** |
| 6 | **Reduced maintenance complexity** | Separation of concerns documented; one place per business rule | `ARCHITECTURE_NOTES.md`, service layer | “Controllers stay thin; changing approval rules means editing `RequestService`, not views.” | **Strong** (qualitative) |
| 7 | **35% page load reduction** | Benchmark project compares naive vs optimized queries | `EnterpriseAssetManager.Benchmarks`, `PERFORMANCE_RESULTS.md` | Measured **33.9%** overall on 2026-06-29 (below 35%). Say: “optimized list pages with server-side filtering, pagination, indexes, and `AsNoTracking`.” | **Medium** — 33.9% measured; do not round up to 35% without re-run |
| 8 | **DBMS efficiency (concurrent searches)** | Indexes on search columns; `IQueryable` + `AsNoTracking`; `AddPerformanceIndexes` migration | `ApplicationDbContext.cs`, `Migrations/*AddPerformanceIndexes*` | “Queries filter and page in SQL Server with indexes on email, asset tags, request status, etc., instead of loading full tables.” | **Strong** (design); concurrent load not load-tested |
| 9 | **xUnit tests** | 34 automated tests, all passing | `EnterpriseAssetManager.Tests/` | “xUnit tests cover hashing, auth, request workflow, assets, users, pagination, and authorization attributes.” | **Strong** |
| 10 | **85% test coverage** | coverlet measured **14.57%** assembly / **~69.7%** services avg | `COVERAGE_RESULTS.md`, `EnterpriseAssetManager.Tests.csproj` | “xUnit + coverlet on service-layer logic; coverage is documented honestly in `COVERAGE_RESULTS.md`.” | **Weak** for “85%” — reword or add tests |
| 11 | **GitHub version control** | `.gitignore` exists; **no `.git` yet** | `GIT_EVIDENCE.md` | “Project is structured for Git/GitHub; I initialize the repo and push before sharing the link.” | **Weak** until `git init` + push |

## Quick risk summary

| Risk | Claims |
|------|--------|
| **Strong** | ASP.NET Core stack, portal workflow, OOP services, xUnit tests, query optimization design |
| **Medium** | 500 users (demo seed), Agile wording, 35% performance (33.9% measured) |
| **Weak / needs proof** | 85% coverage, GitHub history |

## One-line honest pitch

> “AssetOps is an ASP.NET Core MVC portal on SQL Server Express for asset requests and approvals, with a service-layer architecture, indexed EF Core queries, xUnit tests, seeded demo data at 500 users, and documented benchmarks — I describe scale and metrics honestly as demo/test results, not production claims.”
