using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.ClassSessions;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.ClassSessions;

public sealed class ClassSessionService(IClassSessionRepository sessions) : IClassSessionService
{
    public async Task<GeneratedClassSessionsDto> GenerateAsync(
        GenerateClassSessionsCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.CourseSectionId == Guid.Empty
            || command.FromDate == default || command.ToDate < command.FromDate
            || command.ToDate == DateOnly.MaxValue
            || command.ToDate.DayNumber - command.FromDate.DayNumber > 365
            || command.WeeklySlots is null or { Count: < 1 or > 14 }
            || command.ExcludeDates is null)
        {
            throw Invalid("Schedule data is invalid.");
        }

        var section = await sessions.GetSectionAsync(command.CourseSectionId, cancellationToken)
            ?? throw new ClassSessionException(ClassSessionError.SectionNotFound,
                "Course section not found.");
        if (command.FromDate < section.SemesterStartDate || command.ToDate > section.SemesterEndDate
            || command.ExcludeDates.Any(date => date < command.FromDate || date > command.ToDate))
        {
            throw Invalid("Schedule dates must be within the semester.");
        }

        var timeZone = ResolveTimeZone(command.TimeZoneId);
        var slots = new List<(DayOfWeek Day, TimeOnly Start, TimeOnly End, string? Room)>();
        foreach (var slot in command.WeeklySlots)
        {
            if (slot is null
                || !Enum.TryParse<DayOfWeek>(slot.DayOfWeek, true, out var day)
                || !string.Equals(day.ToString(), slot.DayOfWeek, StringComparison.OrdinalIgnoreCase)
                || slot.EndTime <= slot.StartTime
                || slot.EndTime - slot.StartTime > TimeSpan.FromHours(12)
                || slot.Room?.Trim().Length > 50)
            {
                throw Invalid("Weekly slot data is invalid.");
            }

            slots.Add((day, slot.StartTime, slot.EndTime,
                string.IsNullOrWhiteSpace(slot.Room) ? null : slot.Room.Trim()));
        }

        var excluded = command.ExcludeDates.ToHashSet();
        var proposed = new List<ClassSessionEntity>();
        var slotIndexes = new Dictionary<Guid, int>();
        for (var date = command.FromDate; date <= command.ToDate; date = date.AddDays(1))
        {
            if (excluded.Contains(date))
            {
                continue;
            }

            for (var index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];
                if (date.DayOfWeek != slot.Day)
                {
                    continue;
                }

                var startAt = ToUtc(date, slot.Start, timeZone);
                var endAt = ToUtc(date, slot.End, timeZone);
                var id = Guid.NewGuid();
                proposed.Add(new ClassSessionEntity
                {
                    Id = id,
                    CourseSectionId = command.CourseSectionId,
                    StartAt = startAt,
                    EndAt = endAt,
                    Room = slot.Room,
                    Status = "Scheduled"
                });
                slotIndexes.Add(id, index);
                if (proposed.Count > 200)
                {
                    throw Invalid("A schedule can create at most 200 sessions.");
                }
            }
        }

        if (proposed.Count == 0)
        {
            throw Invalid("No class sessions fall within the selected dates.");
        }

        var errors = new Dictionary<string, List<string>>();
        for (var i = 0; i < proposed.Count; i++)
        {
            for (var j = i + 1; j < proposed.Count; j++)
            {
                if (!Overlaps(proposed[i], proposed[j]))
                {
                    continue;
                }

                AddError(errors, $"weeklySlots[{slotIndexes[proposed[i].Id]}]",
                    $"Overlaps another weekly slot on {proposed[i].StartAt:yyyy-MM-dd}.");
                AddError(errors, $"weeklySlots[{slotIndexes[proposed[j].Id]}]",
                    $"Overlaps another weekly slot on {proposed[j].StartAt:yyyy-MM-dd}.");
            }
        }

        ThrowIfConflicts(errors);
        return await sessions.WithScheduleLockAsync(async () =>
        {
            var existing = await sessions.FindOverlapsAsync(
                section.Id,
                section.LecturerId,
                proposed.Select(item => item.Room).Where(room => room is not null)
                    .Select(room => room!.ToUpperInvariant()).Distinct().ToArray(),
                proposed.Min(item => item.StartAt),
                proposed.Max(item => item.EndAt),
                null,
                cancellationToken);
            var conflicts = new Dictionary<string, List<string>>();
            foreach (var item in proposed)
            {
                foreach (var other in existing.Where(other => Overlaps(item, other)))
                {
                    foreach (var message in DescribeConflict(item, other, section.LecturerId))
                    {
                        AddError(conflicts, $"weeklySlots[{slotIndexes[item.Id]}]",
                            $"{item.StartAt:yyyy-MM-ddTHH:mm:ssZ}: {message}");
                    }
                }
            }

            ThrowIfConflicts(conflicts);
            await sessions.CreateBatchAsync(proposed, cancellationToken);
            var result = proposed.Select(ToDto).ToArray();
            return new GeneratedClassSessionsDto(section.Id, result.Length, result);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<ClassSessionDto>> ListAsync(
        Guid courseSectionId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        if (courseSectionId == Guid.Empty)
        {
            throw Invalid("Date range is invalid.");
        }

        var (fromUtc, toUtc) = GetUtcRange(fromDate, toDate, timeZoneId);

        if (await sessions.GetSectionAsync(courseSectionId, cancellationToken) is null)
        {
            throw new ClassSessionException(ClassSessionError.SectionNotFound,
                "Course section not found.");
        }

        var items = await sessions.ListAsync(courseSectionId, fromUtc, toUtc, cancellationToken);
        return items.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyList<LecturerScheduleItemDto>> ListForLecturerAsync(
        Guid lecturerId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        if (lecturerId == Guid.Empty)
        {
            throw Invalid("Lecturer ID is invalid.");
        }

        var (fromUtc, toUtc) = GetUtcRange(fromDate, toDate, timeZoneId);
        if (!await sessions.LecturerExistsAsync(lecturerId, cancellationToken))
        {
            throw new ClassSessionException(ClassSessionError.LecturerNotFound,
                "Lecturer not found.");
        }

        return await sessions.ListForLecturerAsync(lecturerId, fromUtc, toUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<LecturerScheduleItemDto>> ListForCurrentLecturerAsync(
        Guid userId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw Invalid("User ID is invalid.");
        }

        var (fromUtc, toUtc) = GetUtcRange(fromDate, toDate, timeZoneId);
        var lecturerId = await sessions.GetLecturerIdForUserAsync(userId, cancellationToken)
            ?? throw new ClassSessionException(ClassSessionError.LecturerNotFound,
                "Lecturer not found.");
        return await sessions.ListForLecturerAsync(lecturerId, fromUtc, toUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentScheduleItemDto>> ListForCurrentStudentAsync(
        Guid userId,
        DateOnly fromDate,
        DateOnly toDate,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw Invalid("User ID is invalid.");
        }

        var (fromUtc, toUtc) = GetUtcRange(fromDate, toDate, timeZoneId);
        var studentId = await sessions.GetStudentIdForUserAsync(userId, cancellationToken)
            ?? throw new ClassSessionException(ClassSessionError.StudentNotFound,
                "Student not found.");
        return await sessions.ListForStudentAsync(studentId, fromUtc, toUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentScheduleItemDto>> ListUpcomingForCurrentStudentAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || limit is < 1 or > 100)
        {
            throw Invalid("User ID or limit is invalid.");
        }

        var studentId = await sessions.GetStudentIdForUserAsync(userId, cancellationToken)
            ?? throw new ClassSessionException(ClassSessionError.StudentNotFound,
                "Student not found.");
        return await sessions.ListUpcomingForStudentAsync(
            studentId, DateTime.UtcNow, limit, cancellationToken);
    }

    public async Task<ClassSessionDto> UpdateAsync(
        UpdateClassSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty
            || command.StartAtUtc?.Offset != TimeSpan.Zero && command.StartAtUtc is not null
            || command.EndAtUtc?.Offset != TimeSpan.Zero && command.EndAtUtc is not null
            || command.ClearRoom && command.Room is not null
            || command.ClearTopic && command.Topic is not null
            || command.Room is not null && string.IsNullOrWhiteSpace(command.Room)
            || command.Topic is not null && string.IsNullOrWhiteSpace(command.Topic)
            || command.Room?.Trim().Length > 50
            || command.Topic?.Trim().Length > 255
            || command.Status is not null && !new[] { "Scheduled", "Cancelled", "Completed" }
                .Contains(command.Status, StringComparer.OrdinalIgnoreCase)
            || command.StartAtUtc is null && command.EndAtUtc is null
                && command.Room is null && !command.ClearRoom
                && command.Topic is null && !command.ClearTopic && command.Status is null)
        {
            throw Invalid("Class session update is invalid.");
        }

        var timeZone = ResolveTimeZone(command.TimeZoneId);
        return await sessions.WithScheduleLockAsync(async () =>
        {
            var existing = await sessions.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new ClassSessionException(ClassSessionError.SessionNotFound,
                    "Class session not found.");
            var section = await sessions.GetSectionAsync(existing.CourseSectionId, cancellationToken)
                ?? throw new ClassSessionException(ClassSessionError.SectionNotFound,
                    "Course section not found.");
            var startAt = command.StartAtUtc?.UtcDateTime ?? DateTime.SpecifyKind(existing.StartAt, DateTimeKind.Utc);
            var endAt = command.EndAtUtc?.UtcDateTime ?? DateTime.SpecifyKind(existing.EndAt, DateTimeKind.Utc);
            if (endAt <= startAt || endAt - startAt > TimeSpan.FromHours(12))
            {
                throw Invalid("Class session times are invalid.");
            }

            var localStart = TimeZoneInfo.ConvertTimeFromUtc(startAt, timeZone);
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endAt.AddTicks(-1), timeZone);
            if (DateOnly.FromDateTime(localStart) != DateOnly.FromDateTime(localEnd)
                || DateOnly.FromDateTime(localStart) < section.SemesterStartDate
                || DateOnly.FromDateTime(localEnd) > section.SemesterEndDate)
            {
                throw Invalid("Class session must stay within one day of the semester.");
            }

            var updated = new ClassSessionEntity
            {
                Id = existing.Id,
                CourseSectionId = existing.CourseSectionId,
                StartAt = startAt,
                EndAt = endAt,
                Room = command.ClearRoom ? null : command.Room is null ? existing.Room : command.Room.Trim(),
                Topic = command.ClearTopic ? null : command.Topic is null ? existing.Topic : command.Topic.Trim(),
                Status = command.Status is null ? existing.Status :
                    new[] { "Scheduled", "Cancelled", "Completed" }
                        .Single(status => status.Equals(command.Status, StringComparison.OrdinalIgnoreCase))
            };
            if (updated.Status != "Cancelled")
            {
                var overlapping = await sessions.FindOverlapsAsync(
                    section.Id, section.LecturerId,
                    updated.Room is null ? [] : [updated.Room.ToUpperInvariant()],
                    updated.StartAt, updated.EndAt, updated.Id, cancellationToken);
                var conflicts = new Dictionary<string, List<string>>();
                foreach (var other in overlapping.Where(other => Overlaps(updated, other)))
                {
                    foreach (var message in DescribeConflict(updated, other, section.LecturerId))
                    {
                        AddError(conflicts, "startAtUtc", message);
                    }
                }

                ThrowIfConflicts(conflicts);
            }

            if (!await sessions.UpdateAsync(updated, cancellationToken))
            {
                throw new ClassSessionException(ClassSessionError.SessionNotFound,
                    "Class session not found.");
            }

            return ToDto(updated);
        }, cancellationToken);
    }

    private static (DateTime FromUtc, DateTime ToExclusiveUtc) GetUtcRange(
        DateOnly fromDate, DateOnly toDate, string timeZoneId)
    {
        if (fromDate == default || toDate < fromDate || toDate == DateOnly.MaxValue
            || toDate.DayNumber - fromDate.DayNumber > 365)
        {
            throw Invalid("Date range is invalid.");
        }

        var timeZone = ResolveTimeZone(timeZoneId);
        return (ToUtc(fromDate, TimeOnly.MinValue, timeZone),
            ToUtc(toDate.AddDays(1), TimeOnly.MinValue, timeZone));
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw Invalid("Time zone is required.");
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw Invalid("Time zone is invalid.");
        }
    }

    private static DateTime ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo timeZone)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(local) || timeZone.IsAmbiguousTime(local))
        {
            throw Invalid("A schedule time is invalid or ambiguous in the selected time zone.");
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
    }

    private static bool Overlaps(ClassSessionEntity candidate, ClassSessionEntity other) =>
        candidate.StartAt < other.EndAt && other.StartAt < candidate.EndAt;

    private static bool Overlaps(ClassSessionEntity candidate, ExistingScheduleSession other) =>
        candidate.StartAt < other.EndAtUtc && other.StartAtUtc < candidate.EndAt;

    private static IEnumerable<string> DescribeConflict(
        ClassSessionEntity candidate,
        ExistingScheduleSession other,
        Guid lecturerId)
    {
        if (candidate.CourseSectionId == other.CourseSectionId)
        {
            yield return "Course section already has a class at this time.";
        }

        if (lecturerId == other.LecturerId)
        {
            yield return "Lecturer already has a class at this time.";
        }

        if (candidate.Room is not null && other.Room is not null
            && string.Equals(candidate.Room, other.Room, StringComparison.OrdinalIgnoreCase))
        {
            yield return $"Room {candidate.Room} is occupied at this time.";
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors.Add(key, messages);
        }

        if (!messages.Contains(message))
        {
            messages.Add(message);
        }
    }

    private static void ThrowIfConflicts(Dictionary<string, List<string>> errors)
    {
        if (errors.Count > 0)
        {
            throw new ClassSessionException(ClassSessionError.ScheduleConflict,
                "Schedule conflict.", errors.ToDictionary(item => item.Key, item => item.Value.ToArray()));
        }
    }

    private static ClassSessionDto ToDto(ClassSessionEntity session) => new(
        session.Id,
        session.CourseSectionId,
        DateTime.SpecifyKind(session.StartAt, DateTimeKind.Utc),
        DateTime.SpecifyKind(session.EndAt, DateTimeKind.Utc),
        session.Room,
        session.Topic,
        session.Status);

    private static ClassSessionException Invalid(string message) =>
        new(ClassSessionError.InvalidData, message);
}
