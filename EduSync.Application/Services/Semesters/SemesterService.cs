using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Semesters;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.Semesters;

public sealed class SemesterService(ISemesterRepository semesters) : ISemesterService
{
    public async Task<SemesterDto> CreateAsync(
        CreateSemesterCommand command,
        CancellationToken cancellationToken = default)
    {
        var name = command.Name.Trim();
        var academicYear = command.AcademicYear.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100
            || string.IsNullOrWhiteSpace(academicYear) || academicYear.Length > 20
            || command.StartDate == default || command.EndDate <= command.StartDate)
        {
            throw new SemesterException(SemesterError.InvalidData, "Semester data is invalid.");
        }

        if (await semesters.ExistsAsync(academicYear, name, cancellationToken))
        {
            throw new SemesterException(SemesterError.AlreadyExists,
                "Semester already exists for this academic year.");
        }

        var semester = new SemesterEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            AcademicYear = academicYear,
            StartDate = command.StartDate,
            EndDate = command.EndDate
        };
        await semesters.CreateAsync(semester, cancellationToken);
        return new SemesterDto(
            semester.Id,
            semester.Name,
            semester.AcademicYear,
            semester.StartDate,
            semester.EndDate);
    }

    public async Task<IReadOnlyList<SemesterDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await semesters.ListAsync(cancellationToken);
        return entities.Select(semester => new SemesterDto(
            semester.Id,
            semester.Name,
            semester.AcademicYear,
            semester.StartDate,
            semester.EndDate)).ToArray();
    }
}
