using EduSync.Application.Abstractions.Authentication;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure;

public static class HihiStudentSeeder
{
    public static async Task<int> SeedAsync(MyFschoolDemoContext context, IPasswordService passwords)
    {
        Guid[] sectionIds =
        [
            Guid.Parse("10f93700-9a7d-41f7-9a4c-63589b8e909c"),
            Guid.Parse("6f7e04bc-9511-4065-be95-d174ed65026a"),
            Guid.Parse("bee4e621-3958-42a0-940a-dc57d1870146")
        ];
        string[] names =
        [
            "Nguyễn Văn An", "Trần Thị Bình", "Lê Minh Châu", "Phạm Quốc Dũng",
            "Hoàng Thu Hà", "Vũ Đức Huy", "Đặng Ngọc Lan", "Bùi Quang Minh",
            "Đỗ Thanh Phương", "Ngô Anh Tuấn"
        ];
        await using var transaction = await context.Database.BeginTransactionAsync();
        foreach (var sectionId in sectionIds.OrderBy(id => id))
        {
            var resource = $"EduSync.Enrollment.{sectionId:N}";
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @result int;
                EXEC @result = sp_getapplock @Resource = {resource},
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
                IF @result < 0 THROW 51001, 'Could not acquire enrollment lock.', 1;
                """);
        }

        var sections = await context.CourseSections.Where(s => sectionIds.Contains(s.Id))
            .Include(s => s.Course).ToListAsync();
        if (sections.Count != 3 || sections.Any(s => s.Course.CourseName != "HIHI" || s.Status != "Open"))
        {
            throw new InvalidOperationException("Expected all three open HIHI sections.");
        }

        var majorId = await context.Majors.OrderBy(m => m.MajorCode).Select(m => m.Id).FirstAsync();
        var roleId = await context.Roles.Where(r => r.Name == "Student").Select(r => r.Id).SingleAsync();
        var students = new List<Student>();
        for (var index = 0; index < names.Length; index++)
        {
            var code = $"DEMO-HIHI-{index + 1:000}";
            var email = $"demo.hihi.{index + 1:000}@edusync.local";
            var student = await context.Students.Include(s => s.User)
                .SingleOrDefaultAsync(s => s.StudentCode == code);
            if (student is not null)
            {
                if (student.User.Email != email)
                {
                    throw new InvalidOperationException($"Student code {code} is already in use.");
                }
            }
            else
            {
                if (await context.Users.AnyAsync(u => u.Email == email))
                {
                    throw new InvalidOperationException($"Demo email {email} is already in use.");
                }

                var userId = Guid.NewGuid();
                var user = new User
                {
                    Id = userId,
                    Email = email,
                    RoleId = roleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = passwords.Hash(new UserEntity { Id = userId, Email = email },
                        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)))
                };
                student = new Student
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    UserId = userId,
                    StudentCode = code,
                    FullName = names[index],
                    MajorId = majorId,
                    EnrollmentYear = 2026,
                    Status = "Studying",
                    DateOfBirth = new DateOnly(2007, index + 1, 15)
                };
                context.Students.Add(student);
            }

            students.Add(student);
        }

        var added = 0;
        foreach (var section in sections)
        {
            var existing = await context.Enrollments.Where(e => e.CourseSectionId == section.Id)
                .Select(e => e.StudentId).ToListAsync();
            var missing = students.Where(s => !existing.Contains(s.Id)).ToArray();
            if (existing.Count + missing.Length > section.Capacity)
            {
                throw new InvalidOperationException($"Section {section.SectionCode} does not have enough capacity.");
            }

            foreach (var student in missing)
            {
                context.Enrollments.Add(new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    CourseSectionId = section.Id,
                    EnrolledAt = DateTime.UtcNow,
                    Status = "Registered"
                });
                added++;
            }
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return added;
    }
}
