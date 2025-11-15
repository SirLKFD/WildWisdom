<!--
Purpose: Guidance for AI coding agents working on the WildWisdom (ASI.Basecode) repo.
Keep this short (20–50 lines). Include exact file references and examples of common edits.
-->

# Quick instructions for AI coding assistants

- Project type: ASP.NET Core web solution (multiple projects) using EF Core, AutoMapper and a services/repository pattern.
- Solution entry: `ASI.Basecode.sln`. Web app project: `ASI.Basecode.WebApp` (set as startup project).

- Build / run (developer environment): Use Visual Studio 2022 or dotnet CLI from repo root.
  - Local build: `dotnet build ASI.Basecode.sln`
  - Run web app: `dotnet run --project ASI.Basecode.WebApp` (or use provided VS launch)
  - Publish: `dotnet publish ASI.Basecode.sln`

- Important runtime files:
  - `Program.cs` — minimal host builder; StartupConfigurer is used for actual wiring.
  - `Startup*.cs` (`Startup.DI.cs`, `Startup.AutoMapper.cs`, `Startup.Logger.cs`, `Startup.cs`) — app configuration split by concern.

- Dependency injection and service boundaries
  - Business logic lives in `ASI.Basecode.Services` (services implement `I*Service` interfaces).
  - Data access lives in `ASI.Basecode.Data` (repositories under `Data/Repositories` implement `I*Repository`).
  - `Startup.DI.cs` registers services and repositories; use it to see what to mock or change when adding features.

- Common patterns and conventions
  - Services accept repository interfaces + AutoMapper and return view models from `ASI.Basecode.Services.ServiceModels`.
  - Repositories inherit from `BaseRepository<T>` and call `UnitOfWork.SaveChanges()` after mutating data.
  - Password handling: `PasswordManager` (see `ASI.Basecode.Services.Manager.PasswordManager`) — use when creating/updating credentials.
  - AutoMapper: mapping profiles are discovered with `MapConfigurationFactory.Scan(...)` in `Startup.AutoMapper.cs`.

- Controllers & Views
  - MVC controllers and Razor views live in `ASI.Basecode.WebApp/Controllers` and `Views`.
  - Keep controllers thin; business logic should be delegated to services (project convention).

- Localization / Resources
  - UI strings and messages are centralized in `ASI.Basecode.Resources` (resx files under `Views` and `Messages`).

- Testing and quick checks
  - There are no test projects in this repo by default; prefer small, focused manual checks by running the WebApp locally.

- Common edit examples (do these exactly):
  - To add a new service:
    1. Create `IYourService` in `ASI.Basecode.Services.Interfaces` and `YourService` in `ASI.Basecode.Services.Services`.
    2. Register it in `Startup.DI.cs` with `services.AddScoped<IYourService, YourService>();`
  - To add a repository:
    1. Implement interface in `ASI.Basecode.Data/Repositories`, inherit `BaseRepository<T>` and use `UnitOfWork.SaveChanges()` after writes.
    2. Register it in `Startup.DI.cs`.

- Integration points and external dependencies
  - SQL Server connection string is in `ASI.Basecode.WebApp/appsettings.json` under `ConnectionStrings:DefaultConnection`.
  - JWT token config lives in `appsettings.json` under `TokenAuthentication`. `StartupConfigurer` creates signing key and token options.
  - Swagger is enabled at `/swagger` (useful for API surface testing).

- When modifying DB models
  - Update EF Core models in `ASI.Basecode.Data.Models` and create migrations in the Data project; follow the project's migration strategy (no auto-generated migrations found in repo).

- Files to reference for examples
  - Service example: `ASI.Basecode.Services/Services/AdminService.cs`
  - Repository example: `ASI.Basecode.Data/Repositories/UserRepository.cs`
  - DI wiring: `ASI.Basecode.WebApp/Startup.DI.cs`
  - App startup: `ASI.Basecode.WebApp/Program.cs` and `Startup.cs`

If anything here is unclear or you need the agent to follow stricter rules (naming, tests, or commit message format), tell me and I'll iterate.
