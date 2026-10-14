using EduSync.Domain.Entities;
using EduSync.Infrastructure.Models;

namespace EduSync.Infrastructure.Mappings;

public static class EntityMappings
{
    public static AttendanceEntity ToDomainEntity(this Attendance source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new AttendanceEntity
        {
            Id = source.Id,
            EnrollmentId = source.EnrollmentId,
            ClassSessionId = source.ClassSessionId,
            Status = source.Status,
            Note = source.Note,
            RecordedAt = source.RecordedAt,
        };
    }

    public static Attendance ToDatabaseModel(this AttendanceEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Attendance
        {
            Id = source.Id,
            EnrollmentId = source.EnrollmentId,
            ClassSessionId = source.ClassSessionId,
            Status = source.Status,
            Note = source.Note,
            RecordedAt = source.RecordedAt,
        };
    }

    public static ClassSessionEntity ToDomainEntity(this ClassSession source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ClassSessionEntity
        {
            Id = source.Id,
            CourseSectionId = source.CourseSectionId,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
            Room = source.Room,
            Topic = source.Topic,
            Status = source.Status,
        };
    }

    public static ClassSession ToDatabaseModel(this ClassSessionEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ClassSession
        {
            Id = source.Id,
            CourseSectionId = source.CourseSectionId,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
            Room = source.Room,
            Topic = source.Topic,
            Status = source.Status,
        };
    }

    public static ClubEntity ToDomainEntity(this Club source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ClubEntity
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            ContactInfo = source.ContactInfo,
            IsActive = source.IsActive,
        };
    }

    public static Club ToDatabaseModel(this ClubEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Club
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            ContactInfo = source.ContactInfo,
            IsActive = source.IsActive,
        };
    }

    public static ClubMemberEntity ToDomainEntity(this ClubMember source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ClubMemberEntity
        {
            Id = source.Id,
            ClubId = source.ClubId,
            StudentId = source.StudentId,
            JoinedAt = source.JoinedAt,
            Status = source.Status,
        };
    }

    public static ClubMember ToDatabaseModel(this ClubMemberEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ClubMember
        {
            Id = source.Id,
            ClubId = source.ClubId,
            StudentId = source.StudentId,
            JoinedAt = source.JoinedAt,
            Status = source.Status,
        };
    }

    public static CourseEntity ToDomainEntity(this Course source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new CourseEntity
        {
            Id = source.Id,
            CourseCode = source.CourseCode,
            CourseName = source.CourseName,
            Credits = source.Credits,
            Description = source.Description,
        };
    }

    public static Course ToDatabaseModel(this CourseEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Course
        {
            Id = source.Id,
            CourseCode = source.CourseCode,
            CourseName = source.CourseName,
            Credits = source.Credits,
            Description = source.Description,
        };
    }

    public static CourseSectionEntity ToDomainEntity(this CourseSection source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new CourseSectionEntity
        {
            Id = source.Id,
            CourseId = source.CourseId,
            SemesterId = source.SemesterId,
            LecturerId = source.LecturerId,
            SectionCode = source.SectionCode,
            Capacity = source.Capacity,
            Status = source.Status,
        };
    }

    public static CourseSection ToDatabaseModel(this CourseSectionEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new CourseSection
        {
            Id = source.Id,
            CourseId = source.CourseId,
            SemesterId = source.SemesterId,
            LecturerId = source.LecturerId,
            SectionCode = source.SectionCode,
            Capacity = source.Capacity,
            Status = source.Status,
        };
    }

    public static EnrollmentEntity ToDomainEntity(this Enrollment source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new EnrollmentEntity
        {
            Id = source.Id,
            StudentId = source.StudentId,
            CourseSectionId = source.CourseSectionId,
            EnrolledAt = source.EnrolledAt,
            Status = source.Status,
        };
    }

    public static Enrollment ToDatabaseModel(this EnrollmentEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Enrollment
        {
            Id = source.Id,
            StudentId = source.StudentId,
            CourseSectionId = source.CourseSectionId,
            EnrolledAt = source.EnrolledAt,
            Status = source.Status,
        };
    }

    public static EventEntity ToDomainEntity(this Event source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new EventEntity
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            Location = source.Location,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
            RegistrationDeadline = source.RegistrationDeadline,
        };
    }

    public static Event ToDatabaseModel(this EventEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Event
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            Location = source.Location,
            StartAt = source.StartAt,
            EndAt = source.EndAt,
            RegistrationDeadline = source.RegistrationDeadline,
        };
    }

    public static EventRegistrationEntity ToDomainEntity(this EventRegistration source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new EventRegistrationEntity
        {
            Id = source.Id,
            EventId = source.EventId,
            StudentId = source.StudentId,
            RegisteredAt = source.RegisteredAt,
            Status = source.Status,
        };
    }

    public static EventRegistration ToDatabaseModel(this EventRegistrationEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new EventRegistration
        {
            Id = source.Id,
            EventId = source.EventId,
            StudentId = source.StudentId,
            RegisteredAt = source.RegisteredAt,
            Status = source.Status,
        };
    }

    public static GradeEntity ToDomainEntity(this Grade source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new GradeEntity
        {
            Id = source.Id,
            EnrollmentId = source.EnrollmentId,
            GradeComponentId = source.GradeComponentId,
            Score = source.Score,
            GradedAt = source.GradedAt,
            UpdatedAt = source.UpdatedAt,
        };
    }

    public static Grade ToDatabaseModel(this GradeEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Grade
        {
            Id = source.Id,
            EnrollmentId = source.EnrollmentId,
            GradeComponentId = source.GradeComponentId,
            Score = source.Score,
            GradedAt = source.GradedAt,
            UpdatedAt = source.UpdatedAt,
        };
    }

    public static GradeComponentEntity ToDomainEntity(this GradeComponent source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new GradeComponentEntity
        {
            Id = source.Id,
            CourseSectionId = source.CourseSectionId,
            Name = source.Name,
            Weight = source.Weight,
            MaxScore = source.MaxScore,
        };
    }

    public static GradeComponent ToDatabaseModel(this GradeComponentEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new GradeComponent
        {
            Id = source.Id,
            CourseSectionId = source.CourseSectionId,
            Name = source.Name,
            Weight = source.Weight,
            MaxScore = source.MaxScore,
        };
    }

    public static LecturerEntity ToDomainEntity(this Lecturer source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new LecturerEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            LecturerCode = source.LecturerCode,
            FullName = source.FullName,
            Phone = source.Phone,
            AvatarUrl = source.AvatarUrl,
        };
    }

    public static Lecturer ToDatabaseModel(this LecturerEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Lecturer
        {
            Id = source.Id,
            UserId = source.UserId,
            LecturerCode = source.LecturerCode,
            FullName = source.FullName,
            Phone = source.Phone,
            AvatarUrl = source.AvatarUrl,
        };
    }

    public static MajorEntity ToDomainEntity(this Major source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new MajorEntity
        {
            Id = source.Id,
            MajorCode = source.MajorCode,
            MajorName = source.MajorName,
            Description = source.Description,
        };
    }

    public static Major ToDatabaseModel(this MajorEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Major
        {
            Id = source.Id,
            MajorCode = source.MajorCode,
            MajorName = source.MajorName,
            Description = source.Description,
        };
    }

    public static NotificationEntity ToDomainEntity(this Notification source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new NotificationEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            Title = source.Title,
            Content = source.Content,
            Type = source.Type,
            IsRead = source.IsRead,
            CreatedAt = source.CreatedAt,
        };
    }

    public static Notification ToDatabaseModel(this NotificationEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Notification
        {
            Id = source.Id,
            UserId = source.UserId,
            Title = source.Title,
            Content = source.Content,
            Type = source.Type,
            IsRead = source.IsRead,
            CreatedAt = source.CreatedAt,
        };
    }

    public static PasswordResetTokenEntity ToDomainEntity(this PasswordResetToken source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new PasswordResetTokenEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            TokenHash = source.TokenHash,
            ExpiresAt = source.ExpiresAt,
            UsedAt = source.UsedAt,
            CreatedAt = source.CreatedAt,
        };
    }

    public static PasswordResetToken ToDatabaseModel(this PasswordResetTokenEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new PasswordResetToken
        {
            Id = source.Id,
            UserId = source.UserId,
            TokenHash = source.TokenHash,
            ExpiresAt = source.ExpiresAt,
            UsedAt = source.UsedAt,
            CreatedAt = source.CreatedAt,
        };
    }

    public static RefreshTokenEntity ToDomainEntity(this RefreshToken source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new RefreshTokenEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            TokenHash = source.TokenHash,
            Platform = source.Platform,
            DeviceName = source.DeviceName,
            CreatedAt = source.CreatedAt,
            ExpiresAt = source.ExpiresAt,
            RevokedAt = source.RevokedAt,
            ReplacedByTokenId = source.ReplacedByTokenId,
            CreatedByIp = source.CreatedByIp,
        };
    }

    public static RefreshToken ToDatabaseModel(this RefreshTokenEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new RefreshToken
        {
            Id = source.Id,
            UserId = source.UserId,
            TokenHash = source.TokenHash,
            Platform = source.Platform,
            DeviceName = source.DeviceName,
            CreatedAt = source.CreatedAt,
            ExpiresAt = source.ExpiresAt,
            RevokedAt = source.RevokedAt,
            ReplacedByTokenId = source.ReplacedByTokenId,
            CreatedByIp = source.CreatedByIp,
        };
    }

    public static RequestEntity ToDomainEntity(this Request source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new RequestEntity
        {
            Id = source.Id,
            StudentId = source.StudentId,
            RequestType = source.RequestType,
            Title = source.Title,
            Content = source.Content,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = source.Status,
            SubmittedAt = source.SubmittedAt,
            ReviewedBy = source.ReviewedBy,
            ReviewedAt = source.ReviewedAt,
            Response = source.Response,
        };
    }

    public static Request ToDatabaseModel(this RequestEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Request
        {
            Id = source.Id,
            StudentId = source.StudentId,
            RequestType = source.RequestType,
            Title = source.Title,
            Content = source.Content,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = source.Status,
            SubmittedAt = source.SubmittedAt,
            ReviewedBy = source.ReviewedBy,
            ReviewedAt = source.ReviewedAt,
            Response = source.Response,
        };
    }

    public static SemesterEntity ToDomainEntity(this Semester source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new SemesterEntity
        {
            Id = source.Id,
            Name = source.Name,
            AcademicYear = source.AcademicYear,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
        };
    }

    public static Semester ToDatabaseModel(this SemesterEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Semester
        {
            Id = source.Id,
            Name = source.Name,
            AcademicYear = source.AcademicYear,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
        };
    }

    public static StudentEntity ToDomainEntity(this Student source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new StudentEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            StudentCode = source.StudentCode,
            FullName = source.FullName,
            DateOfBirth = source.DateOfBirth,
            Gender = source.Gender,
            Phone = source.Phone,
            AvatarUrl = source.AvatarUrl,
            MajorId = source.MajorId,
            EnrollmentYear = source.EnrollmentYear,
            Status = source.Status,
        };
    }

    public static Student ToDatabaseModel(this StudentEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Student
        {
            Id = source.Id,
            UserId = source.UserId,
            StudentCode = source.StudentCode,
            FullName = source.FullName,
            DateOfBirth = source.DateOfBirth,
            Gender = source.Gender,
            Phone = source.Phone,
            AvatarUrl = source.AvatarUrl,
            MajorId = source.MajorId,
            EnrollmentYear = source.EnrollmentYear,
            Status = source.Status,
        };
    }

    public static UserEntity ToDomainEntity(this User source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new UserEntity
        {
            Id = source.Id,
            Email = source.Email,
            PasswordHash = source.PasswordHash,
            Role = source.Role,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
        };
    }

    public static User ToDatabaseModel(this UserEntity source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new User
        {
            Id = source.Id,
            Email = source.Email,
            PasswordHash = source.PasswordHash,
            Role = source.Role,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
        };
    }
}
