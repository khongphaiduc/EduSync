# Repository Guidelines

Don't write the unit test for this project 

## Project Structure & Module Organization

`EduSync.sln` contains five .NET 8 projects. `EduSync.API/` hosts ASP.NET Core startup, controllers, launch profiles, and app settings. `EduSync.Application/` has folders for DTOs, services, and shared application code. `EduSync.Domain/` has folders for entities, enums, and value objects. `EduSync.Infrastructure/Models/` contains the scaffolded EF Core entities and `MyFschoolDemoContext`. Keep HTTP concerns, including `ApiResponse<T>` in `EduSync.API/Common/`, in the API and database mapping in Infrastructure. Domain classes must end in `Entity` and must never reference or reuse EF models. Map between the two types in Infrastructure. The `Course` and authentication modules show the controller, service, repository, and mapping pattern. `EduSync.Tests/` contains xUnit tests; there is no separate asset directory.

## Build, Test, and Development Commands

Run these from the repository root with the .NET 8 SDK:

- `dotnet restore EduSync.sln` restores NuGet packages.
- `dotnet build EduSync.sln` compiles all four projects.
- `dotnet run --project EduSync.API --launch-profile http` starts the API at `http://localhost:5271`.
- `dotnet test EduSync.sln` runs test projects once they are added; none are present yet.

Use `EduSync.API.http` for the existing Course requests, and update it when adding endpoints.

## Coding Style & Naming Conventions

Use four spaces for C# indentation, PascalCase for types and public members, and camelCase for parameters and local variables. Follow the existing nullable-reference and implicit-using settings. Keep one type per file and match file names to types, for example `Student.cs`. Preserve generated EF entity and mapping names unless a database change is intentional. No repository formatter or lint configuration is present; run `dotnet format EduSync.sln --verify-no-changes` before submitting substantial C# edits.


## Commit & Pull Request Guidelines

This checkout has no Git history, so no established commit style can be confirmed. Use short, imperative subjects such as `Add enrollment validation`. Pull requests should describe the behavior, list verification commands, link an issue when available, and include request/response examples for API changes.

## Security & Configuration

Set `ConnectionStrings__DefaultConnection` and `Jwt__Key` in your local environment before running the API. Keep JWT access and refresh logic behind Application interfaces; store only refresh-token hashes. Keep credentials out of tracked `appsettings*.json` files and never commit production data.
