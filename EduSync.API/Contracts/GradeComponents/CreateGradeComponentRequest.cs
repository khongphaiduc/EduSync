using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.GradeComponents;

public sealed class CreateGradeComponentRequest
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100", ParseLimitsInInvariantCulture = true)]
    public decimal Weight { get; init; }

    [Range(typeof(decimal), "0.01", "999.99", ParseLimitsInInvariantCulture = true)]
    public decimal MaxScore { get; init; } = 10m;
}
