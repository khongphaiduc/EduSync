# EduSync

EduSync is a .NET 8 ASP.NET Core API with a Clean Architecture dependency direction:

- **EduSync.Domain** owns independent business entities. Domain entity names end in `Entity` and never reference EF Core models.
- **EduSync.Application** owns use cases, DTOs, and persistence and authentication interfaces. It depends only on Domain.
- **EduSync.Infrastructure** owns EF Core models, `MyFschoolDemoContext`, repositories, password hashing, JWT issuance, and mapping between database models and domain entities.
- **EduSync.API** owns controllers, request validation, JWT bearer validation, and the `ApiResponse<T>` HTTP envelope.
- **EduSync.Tests** contains xUnit tests for authentication behavior.

The `Course` module shows the controller, service, and repository flow. Authentication uses `Users`, `Roles`, `Students`, and `RefreshTokens`; `Users.RoleId` references `Roles.Id`. The API does not migrate the database automatically. EF Core migrations live in `EduSync.Infrastructure/Migrations`. `InitialSchema` creates the current schema and seeds the three roles for a new database.

For an existing database with `Users.Role`, back it up, then run `Database/Scripts/20261014_SeparateUserRoles.sql` and `Database/Scripts/20261014_RegisterEfBaseline.sql` in that order. The first script preserves existing role assignments; the second records the initial EF migration as already applied so EF will not recreate existing tables. Set `ConnectionStrings__DefaultConnection` and use `dotnet ef database update --project EduSync.Infrastructure --startup-project EduSync.Infrastructure --context MyFschoolDemoContext` for future migrations.

## Run locally

The Development CORS policy allows the Vite frontend at `http://localhost:5173`. For another frontend origin, set `Cors__AllowedOrigins__0` to its exact scheme, host, and port (without a trailing slash). Add more origins with `Cors__AllowedOrigins__1`, etc. Configure production origins through environment variables; bearer-token requests and preflight requests are supported.

Install the .NET 8 SDK and use a SQL Server database matching `MyFschoolDemoContext`. For local Development, save the connection string and a private JWT signing key once with User Secrets:

```powershell
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' '<your SQL Server connection string>' --project EduSync.API
$keyBytes = New-Object byte[] 32
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($keyBytes)
$rng.Dispose()
dotnet user-secrets set 'Jwt:Key' ([Convert]::ToBase64String($keyBytes)) --project EduSync.API

dotnet restore EduSync.sln
dotnet build EduSync.sln
dotnet test EduSync.sln
dotnet run --project EduSync.API --launch-profile http
```

`Jwt:Issuer` and `Jwt:Audience` are set in `EduSync.API/appsettings.json`. Access tokens last 15 minutes and refresh tokens last 30 days by default. The signing key must remain stable across restarts; changing it invalidates previously issued access tokens. User Secrets apply only in Development. Use environment variables for other environments, and do not commit credentials or production data.

## Authentication API

All request bodies below use `Content-Type: application/json`. All responses use `{ "success": true|false, "message": "...", "data": ... }`; validation failures include `errors`.

| Method and URL | Request | Result |
| --- | --- | --- |
| `POST /api/auth/register` | Email, password, student profile | Creates User and Student with role `Student`; returns both tokens. Existing `majorId` required. |
| `POST /api/auth/login` | `email`, `password` | Returns both tokens. |
| `POST /api/auth/refresh` | `refreshToken` | Returns a new access token and refresh token; invalidates the old refresh token. |
| `POST /api/auth/logout` | `refreshToken` | Revokes that refresh token. |
| `GET /api/auth/me` | `Authorization: Bearer <accessToken>` | Returns `id`, `email`, and `role`. |

Registration payload:

```json
{
  "email": "student@example.com",
  "password": "Password123!",
  "studentCode": "STU001",
  "fullName": "Nguyen Van A",
  "majorId": "00000000-0000-0000-0000-000000000001",
  "enrollmentYear": 2026,
  "dateOfBirth": "2007-05-12",
  "gender": "Male",
  "phone": "0900000000"
}
```

Successful registration, login, and refresh return:

```json
{
  "success": true,
  "message": "Logged in.",
  "data": {
    "userId": "00000000-0000-0000-0000-000000000001",
    "email": "student@example.com",
    "role": 3,
    "accessToken": "<jwt>",
    "accessTokenExpiresAtUtc": "2026-10-14T10:15:00Z",
    "refreshToken": "<opaque-refresh-token>",
    "refreshTokenExpiresAtUtc": "2026-11-13T10:00:00Z"
  }
}
```

Registration can return 400 for invalid data or a missing major and 409 for duplicate email or student code. Login returns 401 for invalid credentials and 403 for an inactive account. Refresh returns 401 for an invalid, expired, or previously used token. The server stores only the SHA-256 hash of each refresh token; each successful refresh rotates it. See `EduSync.API/EduSync.API.http` for complete request examples.

Auth responses return `role` as the numeric `UserRole` enum: `Admin = 1`, `Lecturer = 2`, `Student = 3`. The JWT role claim remains the role name for ASP.NET Core authorization.

## Courses

`GET /api/courses?limit=20` (limit 1–100) returns a list of `{ id, courseCode, courseName, credits, description }`; `GET /api/courses/{id}` returns one course or 404. Both use the shared API response envelope.

## Students

`POST /api/students` requires an Admin access token and `Content-Type: application/json`. It creates a `User` with the `Student` role and a linked `Student` record. `majorId` must reference an existing major. The new student can log in with the supplied email and password.

```json
{
  "email": "new.student@example.com",
  "password": "Password123!",
  "studentCode": "STU002",
  "fullName": "Nguyen Van B",
  "majorId": "00000000-0000-0000-0000-000000000001",
  "enrollmentYear": 2026,
  "dateOfBirth": "2007-07-14",
  "gender": "Male",
  "phone": "0900000001"
}
```

The `201 Created` response is:

```json
{
  "success": true,
  "message": "Student created.",
  "data": {
    "id": "00000000-0000-0000-0000-000000000002",
    "userId": "00000000-0000-0000-0000-000000000003",
    "studentCode": "STU002",
    "fullName": "Nguyen Van B",
    "email": "new.student@example.com",
    "majorId": "00000000-0000-0000-0000-000000000001",
    "enrollmentYear": 2026,
    "status": "Studying"
  }
}
```

Invalid data or a missing major returns 400; missing or invalid access tokens return 401; non-admin tokens return 403; duplicate email or student code returns 409. The response never includes the password or tokens.

`GET /api/students?page=1&pageSize=20` requires an access token with the `Admin` role. `page` starts at 1 and `pageSize` is 1–100. Results are ordered by student code and returned as `{ items, page, pageSize, totalCount }` inside the shared API response envelope. Each item includes `id`, `studentCode`, `fullName`, `email`, `majorId`, `majorName`, `enrollmentYear`, `status`, and `avatarUrl`. Missing or invalid tokens return 401; non-admin tokens return 403. See `EduSync.API/EduSync.API.http` for the request.
