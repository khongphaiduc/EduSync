using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Lecturers;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class LecturerRepository(MyFschoolDemoContext context) : ILecturerRepository
{
    public Task<bool> LecturerCodeExistsAsync(string lecturerCode, CancellationToken cancellationToken) =>
        context.Lecturers.AnyAsync(lecturer => lecturer.LecturerCode == lecturerCode, cancellationToken);

    public async Task CreateAsync(UserEntity user, LecturerEntity lecturer, CancellationToken cancellationToken)
    {
        context.Users.Add(user.ToDatabaseModel());
        context.Lecturers.Add(lecturer.ToDatabaseModel());
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new AuthException(AuthError.RegistrationConflict, "Email or lecturer code already exists.");
        }
    }

    public async Task<LecturerListDto> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Lecturers.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new LecturerListDto([], page, pageSize, totalCount);
        }

        var items = await query
            .OrderBy(lecturer => lecturer.LecturerCode)
            .ThenBy(lecturer => lecturer.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(lecturer => new LecturerListItemDto(
                lecturer.Id,
                lecturer.UserId,
                lecturer.LecturerCode,
                lecturer.FullName,
                lecturer.User.Email,
                lecturer.Phone,
                lecturer.User.IsActive))
            .ToArrayAsync(cancellationToken);

        return new LecturerListDto(items, page, pageSize, totalCount);
    }
}
