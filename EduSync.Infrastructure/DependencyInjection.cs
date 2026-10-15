using EduSync.Application.Abstractions.Authentication;
using EduSync.Application.Abstractions.Persistence;
using EduSync.Infrastructure.Authentication;
using EduSync.Infrastructure.Repositories;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<MyFschoolDemoContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILecturerRepository, LecturerRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddSingleton<IPasswordService, IdentityPasswordService>();
        services.AddSingleton<IAuthTokenService, JwtTokenService>();
        return services;
    }
}
