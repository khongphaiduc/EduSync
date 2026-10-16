using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.ClassSessions;

public sealed class GenerateClassSessionsRequest
{
    public DateOnly FromDate { get; init; }

    public DateOnly ToDate { get; init; }

    [Required]
    public string TimeZoneId { get; init; } = "Asia/Ho_Chi_Minh";

    [Required, MinLength(1)]
    public IReadOnlyList<WeeklyScheduleSlotRequest> WeeklySlots { get; init; } = [];

    public IReadOnlyList<DateOnly> ExcludeDates { get; init; } = [];
}
