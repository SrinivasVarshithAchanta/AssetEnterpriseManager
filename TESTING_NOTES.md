# Testing Notes

Supports the resume claim about **xUnit tests** and documents **honest coverage** for the “85%” claim.

**Latest measured results:** `COVERAGE_RESULTS.md` (2026-06-29).

---

## Test project

`EnterpriseAssetManager.Tests` — xUnit + EF Core InMemory (isolated per test).

**Last run:** 34 tests passed, 0 failed.

---

## What is tested

| Area | Tests |
|------|-------|
| Password hashing | Correct/wrong password, not plain text, random salt |
| Auth | Login success/failure, inactive user, role claim on principal |
| Requests | Employee create (Pending), unique numbers, approve, reject (comment required), fulfil, retired asset blocked, cancel rules |
| Assets | Tag uniqueness, duplicate create, retire, search filter, availability count |
| Users | Password hashed on create, duplicate email, email exists, role filter |
| Pagination | Asset pages, user page size, request priority filter |
| Authorization | Approvals exclude Employee; fulfil/admin-only; Users/AuditLogs admin-only |

---

## What is NOT tested automatically

- Razor views / CSS
- Cookie middleware (manual login in browser)
- Live SQL Server queries (InMemory provider in unit tests)
- `DbSeeder` against real database
- Full controller integration / HTTP pipeline

---

## Running tests

```cmd
cd "C:\Users\s_var\OneDrive\Desktop\Personal projects\EnterpriseAssetManager"
dotnet test
```

## Coverage

```cmd
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

Optional HTML report:

```cmd
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

Packages: `coverlet.collector` + `coverlet.msbuild` in test `.csproj`.

---

## Measured coverage (2026-06-29)

| Scope | Line coverage |
|-------|---------------|
| Full assembly | **14.57%** |
| `Services/` (approx. average) | **~69.7%** |

---

## Resume claim: “85% test coverage”

**Current measurement does NOT support a blanket 85% claim.**

### Safe interview wording

> “I wrote xUnit tests for the service layer — authentication, request workflow, asset rules, pagination, and authorization — and measured coverage with coverlet. Coverage is highest on business-logic services; UI is covered with a manual test plan.”

### Do NOT say unless coverage improves

> “The project has 85% test coverage overall.”

To approach 85% on services only, add tests listed in `COVERAGE_RESULTS.md`.
