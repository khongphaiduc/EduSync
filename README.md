# EduSync

EduSync is a .NET 8 ASP.NET Core API with a Clean Architecture dependency direction:

- **EduSync.Domain** owns independent business entities. Domain entity names end in `Entity` and never reference EF Core models.
- **EduSync.Application** owns use cases, DTOs, and persistence and authentication interfaces. It depends only on Domain.
- **EduSync.Infrastructure** owns EF Core models, `MyFschoolDemoContext`, repositories, password hashing, JWT issuance, and mapping between database models and domain entities.
- **EduSync.API** owns controllers, request validation, JWT bearer validation, and the `ApiResponse<T>` HTTP envelope.
- **EduSync.Tests** contains xUnit tests for authentication behavior.

The `Course` module shows the controller, service, and repository flow. Authentication uses `Users`, `Roles`, `Students`, and `RefreshTokens`; `Users.RoleId` references `Roles.Id`. The API applies EF Core migrations when it starts. EF Core migrations live in `EduSync.Infrastructure/Migrations`. `InitialSchema` creates the current schema and seeds the three roles for a new database.

For an existing database with `Users.Role`, back it up, then run `Database/Scripts/20261014_SeparateUserRoles.sql` and `Database/Scripts/20261014_RegisterEfBaseline.sql` in that order. The first script preserves existing role assignments; the second records the initial EF migration as already applied so EF will not recreate existing tables. Set `ConnectionStrings_DefaultConnection` and use `dotnet ef database update --project EduSync.Infrastructure --startup-project EduSync.Infrastructure --context MyFschoolDemoContext` for future migrations.

## Run locally

The Development CORS policy allows the Vite frontend at `http://localhost:5173`. For another frontend origin, set `Cors_AllowedOrigins_0` in `EduSync.API/.env` to its exact scheme, host, and port (without a trailing slash). Add more origins with `Cors_AllowedOrigins_1`, etc. Bearer-token requests and preflight requests are supported.

Install the .NET 8 SDK and use a SQL Server database matching `MyFschoolDemoContext`. Copy `EduSync.API/.env.example` to `EduSync.API/.env`, then set a private signing key in that file. For example, generate one in PowerShell:

```powershell
Copy-Item EduSync.API/.env.example EduSync.API/.env
$keyBytes = New-Object byte[] 32
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($keyBytes)
$rng.Dispose()
$jwtKey = [Convert]::ToBase64String($keyBytes)
(Get-Content EduSync.API/.env) -replace '^Jwt_Key=.*$', "Jwt_Key=$jwtKey" | Set-Content EduSync.API/.env
dotnet restore EduSync.sln
dotnet build EduSync.sln
dotnet run --project EduSync.API --launch-profile http
```

The SQL Server connection string and JWT issuer, audience, key, and token lifetimes are read from `EduSync.API/.env`. Set `ConnectionStrings_DefaultConnection` in that file to your SQL Server connection string. Environment variables with the same single-underscore names override values in the file. Access tokens last 15 minutes and refresh tokens last 30 days by default. The signing key must remain stable across restarts; changing it invalidates previously issued access tokens. `.env` is ignored by Git; do not commit credentials or production data.

To create the first admin account, set `Admin_Email` and `Admin_Password` in `EduSync.API/.env`. After migrations complete, startup creates that account with the `Admin` role if the email does not already exist. The password is hashed with the same service used for regular accounts. Restarting the API does not reset an existing admin password. Remove both `Admin_Email` and `Admin_Password` from `.env` after the account is created.

To bootstrap one lecturer account, set `Lecturer_Email`, `Lecturer_Password`, `Lecturer_Code`, and `Lecturer_FullName` in `EduSync.API/.env` (and optionally `Lecturer_Phone`). Startup uses the existing lecturer creation service and skips the account on later starts. Remove the password from `.env` after creation.

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

## Course sections

`POST /api/course-sections` requires an Admin access token. Send existing `courseId`, `semesterId`, and `lecturerId` values with a `sectionCode` of at most 30 characters and a positive `capacity`. A new section starts with status `Open`. The response is `201 Created` with the section ID and submitted fields. Missing references or invalid data return 400; a section code already used in the same semester returns 409. See `EduSync.API/EduSync.API.http` for a request example.

`GET /api/course-sections?page=1&pageSize=20` also requires an Admin access token. `page` starts at 1 and `pageSize` is 1–100. It returns `items`, `page`, `pageSize`, and `totalCount`; each item includes the course, semester, and lecturer names alongside its IDs, code, capacity, and status. Results are ordered by semester start date (newest first), then section code.

`GET /api/lecturers/me/course-sections?page=1&pageSize=20` requires a Lecturer access token and returns only the sections assigned to that lecturer. The lecturer is resolved from the token's user ID. It uses the same pagination, item fields, and ordering as the Admin section list.

`POST /api/course-sections/{sectionId}/students` requires an Admin access token and a JSON body with `studentId`. It creates a `Registered` enrollment and returns its ID and enrollment time. The section must be `Open`, the student must be active and `Studying`, and the section must have capacity. Missing references return 404; a duplicate enrollment, closed section, or full section returns 409. Enrollment checks and creation run under a per-section database lock to prevent overbooking.

`GET /api/course-sections/{sectionId}/students?page=1&pageSize=20` accepts an Admin token or a Lecturer token for the lecturer assigned to that section. It returns the enrolled students ordered by student code. The response contains `items`, `page`, `pageSize`, and `totalCount`; each item has the enrollment ID, student ID, code, name, email, student status, enrollment status, and enrollment time. A missing section returns 404, and a lecturer assigned to another section receives 403.

## Class sessions

Admins can call `POST /api/course-sections/{sectionId}/sessions/generate` with `fromDate`, `toDate`, `timeZoneId`, `weeklySlots`, and optional `excludeDates`. Each weekly slot has `dayOfWeek` (English weekday name), `startTime`, `endTime`, and optional `room`. The date range must fit within the section's semester, span at most 366 days, and generate at most 200 sessions. Sessions are stored as UTC instants. The API creates the entire batch or returns 409 with conflict details if the section, lecturer, or room is already booked. Calling it again with the same schedule returns 409.

`GET /api/course-sections/{sectionId}/sessions?from=2026-09-01&to=2026-12-15&timeZoneId=Asia/Ho_Chi_Minh` returns sessions overlapping that local date range, ordered by start time. `PATCH /api/class-sessions/{id}` changes one session: send any of `startAtUtc`, `endAtUtc`, `room`, `topic`, or `status` (`Scheduled`, `Cancelled`, `Completed`). Time values must have a UTC offset of zero. `clearRoom` or `clearTopic` removes an existing value. `timeZoneId` defaults to `Asia/Ho_Chi_Minh` and is used to check that a rescheduled session remains within the semester. All three endpoints require an Admin token. See `EduSync.API/EduSync.API.http` for examples.

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

Add `search` to the student list query to find partial matches in student names or student codes, for example `GET /api/students?search=Nguyen&page=1&pageSize=20`. The result keeps the same pagination and response format. The search text is trimmed and may contain up to 150 characters.

To populate the three local HIHI sections (SWE999, SWE222, SWE998), run the API once with `Seed_HihiStudents=true` in the process environment. This inserts ten demo students (`DEMO-HIHI-001` through `DEMO-HIHI-010`) into each section and then exits. Existing demo students and enrollments are reused. The operation uses one transaction and checks each section's capacity. Keep the flag unset when starting the API normally.

## Grade components

`POST /api/course-sections/{sectionId}/grade-components` requires an Admin token. Send `{ "name": "Midterm", "weight": 30, "maxScore": 10 }`. It returns 201 with `{ id, courseSectionId, name, weight, maxScore }` in the shared response envelope. Names are trimmed, required, and limited to 100 characters. Weight is a percentage greater than zero and at most 100; maxScore defaults to 10 and must be greater than zero and at most 999.99. Both numbers accept at most two decimal places. Missing sections return 404; duplicate names or total weight exceeding 100 return 409. Checks and insertion use a transaction and a per-section lock. A total below 100 is allowed while configuring components.

## Requests API

`GET /api/requests?page=1&pageSize=20` requires an Admin access token. Optional `status`, `requestType`, and `studentId` query parameters filter the list. Results are ordered by submission time (newest first) and returned as `{ items, page, pageSize, totalCount }` inside the shared API response envelope. Each item includes the student code and name, request type, title, content, dates, status, submission time, and review details. See `EduSync.API/EduSync.API.http` for an example.

For local demo data, set `Seed_DemoRequests=true` in `EduSync.API/.env` and start the API. The startup seeder creates a disabled sample student and a major only if needed, then inserts three requests (Pending, Approved, and Rejected). The sample requests have fixed IDs, so later starts do not duplicate them. Leave this setting off outside a demo database.
