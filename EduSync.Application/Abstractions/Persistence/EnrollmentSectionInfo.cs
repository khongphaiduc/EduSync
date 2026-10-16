namespace EduSync.Application.Abstractions.Persistence;

public sealed record EnrollmentSectionInfo(
    Guid Id, Guid LecturerUserId, int Capacity, string Status, int EnrollmentCount);
