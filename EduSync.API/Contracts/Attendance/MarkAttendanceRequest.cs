using System.ComponentModel.DataAnnotations;

namespace EduSync.API.Contracts.Attendance;

public sealed class MarkAttendanceRequest
{
    [Required, MinLength(1)]
    public IReadOnlyList<MarkAttendanceItemRequest> Items { get; init; } = [];
}
