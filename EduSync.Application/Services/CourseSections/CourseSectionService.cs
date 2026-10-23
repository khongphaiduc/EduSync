using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.CourseSections;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.CourseSections;

public sealed class CourseSectionService(ICourseSectionRepository sections) : ICourseSectionService
{
    public Task<CourseSectionListDto> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePagination(page, pageSize);
        return sections.ListAsync(page, pageSize, cancellationToken);
    }

    public Task<CourseSectionListDto> ListForLecturerAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        ValidatePagination(page, pageSize);
        return sections.ListForLecturerAsync(userId, page, pageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentSemesterCourseDto>> ListForStudentSemesterAsync(
        Guid userId,
        Guid semesterId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || semesterId == Guid.Empty)
        {
            throw new CourseSectionException(CourseSectionError.InvalidData,
                "User ID and semester ID are required.");
        }

        if (!await sections.SemesterExistsAsync(semesterId, cancellationToken))
        {
            throw new CourseSectionException(CourseSectionError.SemesterNotFound,
                "Semester does not exist.");
        }

        if (!await sections.StudentExistsForUserAsync(userId, cancellationToken))
        {
            throw new CourseSectionException(CourseSectionError.StudentNotFound,
                "Student does not exist.");
        }

        return await sections.ListForStudentSemesterAsync(userId, semesterId, cancellationToken);
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }
    }

    public async Task<CourseSectionDto> CreateAsync(
        CreateCourseSectionCommand command,
        CancellationToken cancellationToken = default)
    {
        var sectionCode = command.SectionCode?.Trim();
        if (command.CourseId == Guid.Empty || command.SemesterId == Guid.Empty
            || command.LecturerId == Guid.Empty || string.IsNullOrWhiteSpace(sectionCode)
            || sectionCode.Length > 30 || command.Capacity < 1)
        {
            throw new CourseSectionException(CourseSectionError.InvalidData,
                "Course section data is invalid.");
        }

        if (!await sections.CourseExistsAsync(command.CourseId, cancellationToken))
        {
            throw new CourseSectionException(CourseSectionError.CourseNotFound, "Course does not exist.");
        }

        if (!await sections.SemesterExistsAsync(command.SemesterId, cancellationToken))
        {
            throw new CourseSectionException(CourseSectionError.SemesterNotFound, "Semester does not exist.");
        }

        if (!await sections.LecturerExistsAsync(command.LecturerId, cancellationToken))
        {
            throw new CourseSectionException(CourseSectionError.LecturerNotFound, "Lecturer does not exist.");
        }

        if (await sections.SectionCodeExistsAsync(command.SemesterId, sectionCode, cancellationToken))
        {
            throw new CourseSectionException(CourseSectionError.AlreadyExists,
                "Section code already exists in this semester.");
        }

        var section = new CourseSectionEntity
        {
            Id = Guid.NewGuid(),
            CourseId = command.CourseId,
            SemesterId = command.SemesterId,
            LecturerId = command.LecturerId,
            SectionCode = sectionCode,
            Capacity = command.Capacity,
            Status = "Open"
        };
        await sections.CreateAsync(section, cancellationToken);
        return new CourseSectionDto(section.Id, section.CourseId, section.SemesterId,
            section.LecturerId, section.SectionCode, section.Capacity, section.Status);
    }
}
