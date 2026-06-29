# Architecture Notes

Supports the resume claim: *"applied object-oriented programming principles to design modular backend services."*

## Layers
```
Controllers  ->  Services (interfaces)  ->  ApplicationDbContext (EF Core)  ->  SQL Server
   |                  |
ViewModels        Business rules
```

## Encapsulation
Each entity owns its data and validation annotations (`Models/`). Services own behavior and state transitions; controllers and views never mutate entities directly outside a service. For example, only `RequestService` knows how a request moves from Pending to Fulfilled.

## Abstraction through interfaces
Every service has an interface:
- `IPasswordHasher` / `PasswordHasher`
- `IAuthService` / `AuthService`
- `IAssetService` / `AssetService`
- `IRequestService` / `RequestService`
- `IUserService` / `UserService`
- `IAuditService` / `AuditService`

Controllers depend on the interface, not the concrete class, so behavior can change or be mocked without touching the controller.

## Separation of concerns
- **Controllers** — HTTP only: validate input, call a service, return a view or redirect. They are intentionally thin.
- **Services** — business rules: uniqueness checks, approval/fulfilment workflow, hashing, auditing.
- **Data** — `ApplicationDbContext` plus relationship/index configuration and seeding.
- **ViewModels** — shape data for forms and lists, with DataAnnotations for validation.
- **Views** — presentation only; status badges and pagination come from small helpers/partials.

## Dependency injection
All services are registered in `Program.cs` as scoped:
```csharp
builder.Services.AddScoped<IAssetService, AssetService>();
// ...etc
```
ASP.NET Core injects them into controllers via constructors. The same pattern lets the tests construct services directly against an in-memory database.

## How this reduces maintenance complexity
- A rule changes in exactly one place (the owning service).
- Controllers are small and similar, so they are easy to read.
- Interfaces make the code testable, which catches regressions early.
- Adding a feature usually means: add a service method, a thin action, and a view — without rewiring existing code.
