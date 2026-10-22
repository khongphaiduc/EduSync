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
}
