using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.Grades;

namespace EduSync.Application.Services.Grades;

public sealed class SectionGradeService(ISectionGradeRepository grades) : ISectionGradeService
{
    public async Task<SectionGradeListDto> ListAsync(
        Guid sectionId,
        int page,
        int pageSize,
        Guid? lecturerUserId,
        CancellationToken cancellationToken = default)
    {
        if (sectionId == Guid.Empty || page < 1 || pageSize is < 1 or > 100)
        {
            throw new GradeException(GradeError.InvalidData,
                "Course section ID or pagination is invalid.");
        }

        var section = await grades.GetSectionAsync(sectionId, cancellationToken)
            ?? throw new GradeException(GradeError.SectionNotFound,
                "Course section does not exist.");
        if (lecturerUserId is not null && section.LecturerUserId != lecturerUserId.Value)
        {
            throw new GradeException(GradeError.NotAssigned,
                "Lecturer is not assigned to this course section.");
        }

        return await grades.ListAsync(sectionId, page, pageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<SavedGradeDto>> SaveAsync(
        Guid sectionId,
        Guid? lecturerUserId,
        IReadOnlyList<SaveGradeItem> items,
        CancellationToken cancellationToken = default)
    {
        if (sectionId == Guid.Empty || items is null or { Count: < 1 or > 500 }
            || items.Any(item => item is null || item.EnrollmentId == Guid.Empty
                || item.GradeComponentId == Guid.Empty
                || item.Score is < 0 or > 999.99m
                || item.Score is not null && decimal.Round(item.Score.Value, 2) != item.Score.Value)
            || items.Select(item => (item.EnrollmentId, item.GradeComponentId)).Distinct().Count() != items.Count)
        {
            throw new GradeException(GradeError.InvalidData,
                "Grade items must be unique and contain valid IDs and scores with at most two decimal places.");
        }

        var section = await grades.GetSectionAsync(sectionId, cancellationToken)
            ?? throw new GradeException(GradeError.SectionNotFound,
                "Course section does not exist.");
        if (lecturerUserId is not null && section.LecturerUserId != lecturerUserId.Value)
        {
            throw new GradeException(GradeError.NotAssigned,
                "Lecturer is not assigned to this course section.");
        }

        return await grades.SaveAsync(sectionId, items, cancellationToken);
    }
}
