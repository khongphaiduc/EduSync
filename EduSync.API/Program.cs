using System.Text;
using System.Threading.RateLimiting;
using EduSync.API.Common;
using EduSync.Application.Abstractions.Authentication;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Services.Auth;
using EduSync.Application.Services.Attendance;
using EduSync.Application.Services.ClassSessions;
using EduSync.Application.Services.Courses;
using EduSync.Application.Services.CourseSections;
using EduSync.Application.Services.Enrollments;
using EduSync.Application.Services.GradeComponents;
using EduSync.Application.Services.Grades;
using EduSync.Application.Services.Lecturers;
using EduSync.Application.Services.Majors;
using EduSync.Application.Services.Requests;
using EduSync.Application.Services.Semesters;
using EduSync.Application.Services.Students;
using EduSync.Infrastructure;
using EduSync.Infrastructure.Authentication;
using EduSync.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EduSync.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
        if (!File.Exists(envPath))
        {
            envPath = Path.Combine(builder.Environment.ContentRootPath, "EduSync.API", ".env");
        }

        builder.Configuration.AddInMemoryCollection(
            DotEnvConfiguration.Read(envPath));
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddInMemoryCollection(DotEnvConfiguration.ReadEnvironment());

        builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => error.ErrorMessage)
                            .ToArray());

                return new BadRequestObjectResult(
                    ApiResponse<object?>.Failed("Validation failed.", errors));
            };
        });

        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration is required.");
        if (string.IsNullOrWhiteSpace(jwt.Key))
        {
            throw new InvalidOperationException(
                "Jwt:Key is missing. Set Jwt_Key in EduSync.API/.env or the environment.");
        }

        if (string.IsNullOrWhiteSpace(jwt.Issuer)
            || string.IsNullOrWhiteSpace(jwt.Audience)
            || Encoding.UTF8.GetByteCount(jwt.Key) < 32
            || jwt.AccessTokenMinutes is < 1 or > 60
            || jwt.RefreshTokenDays is < 1 or > 90)
        {
            throw new InvalidOperationException(
                "Jwt requires an issuer, audience, a key of at least 32 bytes, and valid token lifetimes.");
        }

        builder.Services.AddSingleton(jwt);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object?>.Failed("Authentication required or access token invalid."));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object?>.Failed("Access denied."));
                    }
                };
            });
        builder.Services.AddAuthorization();
        var frontendOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
            .WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse<object?>.Failed("Too many authentication requests."),
                    cancellationToken);
            };
        });

        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<ICourseSectionService, CourseSectionService>();
        builder.Services.AddScoped<IClassSessionService, ClassSessionService>();
        builder.Services.AddScoped<IAttendanceService, AttendanceService>();
        builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
        builder.Services.AddScoped<IGradeComponentService, GradeComponentService>();
        builder.Services.AddScoped<ISectionGradeService, SectionGradeService>();
        builder.Services.AddScoped<ILecturerService, LecturerService>();
        builder.Services.AddScoped<IMajorService, MajorService>();
        builder.Services.AddScoped<IRequestService, RequestService>();
        builder.Services.AddScoped<ISemesterService, SemesterService>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings_DefaultConnection is missing. Set it in {envPath} or the environment.");
        }

        builder.Services.AddInfrastructure(connectionString);

        var app = builder.Build();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyFschoolDemoContext>();
              await dbContext.Database.MigrateAsync();
            if (app.Configuration.GetValue<bool>("Seed:HihiStudents"))
            {
                var count = await HihiStudentSeeder.SeedAsync(
                    dbContext, scope.ServiceProvider.GetRequiredService<IPasswordService>());
                app.Logger.LogInformation("Added {Count} HIHI demo enrollments.", count);
                return;
            }
            var created = await AdminAccountBootstrap.SeedAsync(
                app.Configuration,
                scope.ServiceProvider.GetRequiredService<IAuthRepository>(),
                scope.ServiceProvider.GetRequiredService<IPasswordService>());
            if (created)
            {
                app.Logger.LogInformation("Initial admin account created.");
            }

            var lecturerCreated = await LecturerAccountBootstrap.SeedAsync(
                app.Configuration,
                scope.ServiceProvider.GetRequiredService<IAuthRepository>(),
                scope.ServiceProvider.GetRequiredService<ILecturerService>());
            if (lecturerCreated)
            {
                app.Logger.LogInformation("Initial lecturer account created.");
            }

            if (app.Configuration.GetValue<bool>("Seed:DemoRequests"))
            {
                var count = await DemoRequestSeeder.SeedAsync(
                    dbContext,
                    scope.ServiceProvider.GetRequiredService<IPasswordService>());
                app.Logger.LogInformation("Added {Count} demo requests.", count);
            }
        }

        app.UseExceptionHandler();
        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            var message = response.StatusCode switch
            {
                StatusCodes.Status404NotFound => "Resource not found.",
                StatusCodes.Status405MethodNotAllowed => "Method not allowed.",
                _ => "Request failed."
            };

            await response.WriteAsJsonAsync(ApiResponse<object?>.Failed(message));
        });
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseRouting();
        app.UseCors("Frontend");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
