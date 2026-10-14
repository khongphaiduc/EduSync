using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduSync.Infrastructure.Models;

public sealed class MyFschoolDemoContextFactory : IDesignTimeDbContextFactory<MyFschoolDemoContext>
{
    public MyFschoolDemoContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection before running EF Core commands.");
        }

        var options = new DbContextOptionsBuilder<MyFschoolDemoContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new MyFschoolDemoContext(options);
    }
}
