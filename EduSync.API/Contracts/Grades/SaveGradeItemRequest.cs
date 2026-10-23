using System.Text.Json.Serialization;

namespace EduSync.API.Contracts.Grades;

public sealed class SaveGradeItemRequest
{
    public Guid EnrollmentId { get; init; }

    public Guid GradeComponentId { get; init; }

    [JsonRequired]
    public decimal? Score { get; init; }
}
