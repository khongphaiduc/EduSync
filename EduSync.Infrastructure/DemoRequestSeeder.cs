using EduSync.Application.Abstractions.Authentication;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure;

public static class DemoRequestSeeder
{
    private static readonly Guid[] RequestIds =
    [
        Guid.Parse("f2c29a51-baf8-4cf8-a843-b8e7636d0001"),
        Guid.Parse("f2c29a51-baf8-4cf8-a843-b8e7636d0002"),
        Guid.Parse("f2c29a51-baf8-4cf8-a843-b8e7636d0003")
    ];

    public static async Task<int> SeedAsync(
        MyFschoolDemoContext context,
        IPasswordService passwords,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var existingIds = await context.Requests.AsNoTracking()
            .Where(request => RequestIds.Contains(request.Id))
            .Select(request => request.Id)
            .ToArrayAsync(cancellationToken);
        if (existingIds.Length == RequestIds.Length)
        {
            return 0;
        }

        var majorId = await context.Majors.AsNoTracking()
            .OrderBy(major => major.MajorCode)
            .Select(major => (Guid?)major.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (majorId is null)
        {
            var major = new Major
            {
                Id = Guid.NewGuid(),
                MajorCode = "DEMO-IT",
                MajorName = "Cong nghe thong tin",
                Description = "Demo data for request examples"
            };
            context.Majors.Add(major);
            majorId = major.Id;
        }

        const string demoEmail = "demo.requests.student@edusync.local";
        var studentId = await context.Students.AsNoTracking()
            .Where(student => student.User.Email == demoEmail)
            .Select(student => (Guid?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (studentId is null)
        {
            var roleId = await context.Roles.AsNoTracking()
                .Where(role => role.Name == "Student")
                .Select(role => role.Id)
                .SingleAsync(cancellationToken);
            var userId = Guid.NewGuid();
            var user = new UserEntity { Id = userId, Email = demoEmail, RoleId = roleId };
            context.Users.Add(new User
            {
                Id = userId,
                Email = demoEmail,
                PasswordHash = passwords.Hash(user, Guid.NewGuid().ToString("N")),
                RoleId = roleId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            });
            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StudentCode = "DEMO-REQ-001",
                FullName = "Nguyen Van Mau",
                MajorId = majorId.Value,
                EnrollmentYear = DateTime.UtcNow.Year,
                Status = "Studying"
            };
            context.Students.Add(student);
            studentId = student.Id;
        }

        var reviewerId = await context.Users.AsNoTracking()
            .Where(user => user.Role.Name == "Admin")
            .Select(user => (Guid?)user.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var samples = new[]
        {
            new Request
            {
                Id = RequestIds[0], StudentId = studentId.Value,
                RequestType = "Leave", Title = "Xin nghi hoc",
                Content = "Xin nghi hoc mot ngay vi ly do suc khoe.",
                StartDate = DateOnly.FromDateTime(now.AddDays(3)),
                EndDate = DateOnly.FromDateTime(now.AddDays(3)),
                Status = "Pending", SubmittedAt = now.AddDays(-1)
            },
            new Request
            {
                Id = RequestIds[1], StudentId = studentId.Value,
                RequestType = "Certificate", Title = "Xin giay xac nhan sinh vien",
                Content = "Can giay xac nhan sinh vien de hoan thien ho so vay von.",
                Status = "Approved", SubmittedAt = now.AddDays(-5),
                ReviewedBy = reviewerId, ReviewedAt = reviewerId is null ? null : now.AddDays(-4),
                Response = "Da chap thuan. Sinh vien den phong dao tao nhan giay."
            },
            new Request
            {
                Id = RequestIds[2], StudentId = studentId.Value,
                RequestType = "Leave", Title = "Xin nghi hoc dai ngay",
                Content = "Xin nghi hoc mot tuan de giai quyet viec ca nhan.",
                StartDate = DateOnly.FromDateTime(now.AddDays(7)),
                EndDate = DateOnly.FromDateTime(now.AddDays(13)),
                Status = "Rejected", SubmittedAt = now.AddDays(-10),
                ReviewedBy = reviewerId, ReviewedAt = reviewerId is null ? null : now.AddDays(-9),
                Response = "Vui long bo sung minh chung va nop lai don."
            }
        };

        var newSamples = samples.Where(request => !existingIds.Contains(request.Id)).ToArray();
        context.Requests.AddRange(newSamples);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return newSamples.Length;
    }
}
