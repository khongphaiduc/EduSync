using System.Text;
using System.Threading.RateLimiting;
using EduSync.API.Common;
using EduSync.Application.Services.Auth;
using EduSync.Application.Services.Courses;
using EduSync.Application.Services.Students;
using EduSync.Infrastructure;
using EduSync.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace EduSync.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
                "Jwt:Key is missing. Configure it with User Secrets or Jwt__Key.");
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
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddInfrastructure(
            builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required."));

        var app = builder.Build();

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
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("Frontend");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
