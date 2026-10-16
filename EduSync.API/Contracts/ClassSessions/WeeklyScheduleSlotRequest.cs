using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.ClassSessions;

public sealed class WeeklyScheduleSlotRequest
{
    [Required]
    public string DayOfWeek { get; init; } = string.Empty;

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    [StringLength(50)]
    public string? Room { get; init; }
}
