using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.ClassSessions;

public sealed class UpdateClassSessionRequest
{
    public DateTimeOffset? StartAtUtc { get; init; }

    public DateTimeOffset? EndAtUtc { get; init; }

    [StringLength(50)]
    public string? Room { get; init; }

    public bool ClearRoom { get; init; }

    [StringLength(255)]
    public string? Topic { get; init; }

    public bool ClearTopic { get; init; }

    public string? Status { get; init; }

    [Required]
    public string TimeZoneId { get; init; } = "Asia/Ho_Chi_Minh";
}
