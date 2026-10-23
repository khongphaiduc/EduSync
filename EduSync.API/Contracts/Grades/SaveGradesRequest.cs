using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Grades;

public sealed class SaveGradesRequest
{
    [Required, MinLength(1), MaxLength(500)]
    public IReadOnlyList<SaveGradeItemRequest> Items { get; init; } = [];
}
