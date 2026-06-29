# Coverage Results

> Last measured: **2026-06-29** on this machine using `dotnet test --collect:"XPlat Code Coverage"`.

## How to run coverage

From the solution root:

```cmd
cd "C:\Users\s_var\OneDrive\Desktop\Personal projects\EnterpriseAssetManager"
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

With coverlet.msbuild (writes to `EnterpriseAssetManager.Tests/TestResults/coverage/`):

```cmd
dotnet test /p:CollectCoverage=true
```

## HTML report (optional)

```cmd
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

Open `CoverageReport\index.html` in a browser.

## Measured results (2026-06-29)

| Scope | Line coverage | Branch coverage | Method coverage |
|-------|---------------|-----------------|-----------------|
| **Entire `EnterpriseAssetManager` assembly** | **14.57%** | 8.36% | 28.06% |
| **`Services/` classes (approx. average)** | **~69.7%** | varies | varies |

**Tests run:** 34 passed, 0 failed.

Coverage XML locations:
- `TestResults/<run-id>/coverage.cobertura.xml`
- `EnterpriseAssetManager.Tests/TestResults/coverage/coverage.cobertura.xml`

## What is covered

- `PasswordHasher` — verify/hash behavior
- `AuthService` — login success/failure, inactive user, claims principal
- `RequestService` — create, approve, reject, fulfil, cancel, request numbers
- `AssetService` — paging, search, tag uniqueness, retire, availability counts
- `UserService` — create, email uniqueness, role filter, password hashing on create
- `AuthorizationPolicyTests` — controller `[Authorize]` roles (employee cannot approve)

## What is NOT covered (by design)

- Razor views, CSS, layout
- `Program.cs` startup pipeline
- `DbSeeder` / migration startup path against live SQL Server
- Most controller actions (thin wrappers)
- `AuditService` read paths beyond what other tests trigger indirectly

## Resume claim: “85% test coverage”

**The current measured full-assembly coverage is 14.57%.**  
**The `Services/` folder averages ~69.7% line coverage** — meaningful, but **below 85%**.

### Safe interview wording

> “I used xUnit with coverlet to test the service layer — password hashing, authentication, request approval workflow, asset rules, pagination, and authorization policies. Coverage is strongest on business-logic services; UI and startup glue are tested manually.”

### Do NOT say unless you expand tests

> “The project has 85% test coverage” — unless a fresh `dotnet test --collect:"XPlat Code Coverage"` run shows ≥85% for the layer you are claiming.

### To move toward 85% on services

Add tests for: `AuditService.GetPagedAsync`, category CRUD in `AssetService`, `UserService.UpdateAsync` / `SetActiveAsync`, and edge cases in `RequestService.GetForApprovalAsync` date filters.
