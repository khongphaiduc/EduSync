# EduSync

EduSync is a .NET 8 ASP.NET Core API with a Clean Architecture dependency direction:

- **EduSync.Domain** owns independent business entities. Domain entity names end in `Entity` and never reference EF Core models.
- **EduSync.Application** owns use cases, DTOs, and persistence and authentication interfaces. It depends only on Domain.
- **EduSync.Infrastructure** owns EF Core models, `MyFschoolDemoContext`, repositories, password hashing, JWT issuance, and mapping between database models and domain entities.
- **EduSync.API** owns controllers, request validation, JWT bearer validation, and the `ApiResponse<T>` HTTP envelope.
- **EduSync.Tests** contains xUnit tests for authentication behavior.

The `Course` module shows the controller, service, and repository flow. Authentication uses the existing `Users`, `Students`, and `RefreshTokens` tables; the API does not create or migrate the database automatically.

## Run locally

Install the .NET 8 SDK and use a SQL Server database matching `MyFschoolDemoContext`. Set the connection string and a private JWT signing key before starting the API:

```powershell
$env:ConnectionStrings__DefaultConnection = '<your SQL Server connection string>'
$env:Jwt__Key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet restore EduSync.sln
dotnet build EduSync.sln
dotnet test EduSync.sln
dotnet run --project EduSync.API --launch-profile http
```

`Jwt:Issuer` and `Jwt:Audience` are set in `EduSync.API/appsettings.json`. Access tokens last 15 minutes and refresh tokens last 30 days by default. Keep the signing key stable across restarts and servers. Do not commit credentials or production data.

## Authentication API

All request bodies below use `Content-Type: application/json`. All responses use `{ "success": true|false, "message": "...", "data": ... }`; validation failures include `errors`.

| Method and URL | Request | Result |
| --- | --- | --- |
| `POST /api/auth/register` | Email, password, student profile, platform | Creates User and Student with role `Student`; returns both tokens. Existing `majorId` required. |
| `POST /api/auth/login` | `email`, `password`, optional `platform`, `deviceName` | Returns both tokens. |
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
  "phone": "0900000000",
  "platform": "Web",
  "deviceName": "Chrome"
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
    "role": "Student",
    "accessToken": "<jwt>",
    "accessTokenExpiresAtUtc": "2026-10-14T10:15:00Z",
    "refreshToken": "<opaque-refresh-token>",
    "refreshTokenExpiresAtUtc": "2026-11-13T10:00:00Z"
  }
}
```

Registration can return 400 for invalid data or a missing major and 409 for duplicate email or student code. Login returns 401 for invalid credentials and 403 for an inactive account. Refresh returns 401 for an invalid, expired, or previously used token. The server stores only the SHA-256 hash of each refresh token; each successful refresh rotates it. See `EduSync.API/EduSync.API.http` for complete request examples.

## Courses

`GET /api/courses?limit=20` (limit 1–100) returns a list of `{ id, courseCode, courseName, credits, description }`; `GET /api/courses/{id}` returns one course or 404. Both use the shared API response envelope.
