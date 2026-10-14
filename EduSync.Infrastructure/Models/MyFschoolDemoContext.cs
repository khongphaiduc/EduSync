using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Models;

public partial class MyFschoolDemoContext : DbContext
{
    public MyFschoolDemoContext()
    {
    }

    public MyFschoolDemoContext(DbContextOptions<MyFschoolDemoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<ClassSession> ClassSessions { get; set; }

    public virtual DbSet<Club> Clubs { get; set; }

    public virtual DbSet<ClubMember> ClubMembers { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventRegistration> EventRegistrations { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<GradeComponent> GradeComponents { get; set; }

    public virtual DbSet<Lecturer> Lecturers { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendance");

            entity.HasIndex(e => e.ClassSessionId, "IX_Attendance_SessionId");

            entity.HasIndex(e => new { e.EnrollmentId, e.ClassSessionId }, "UQ_Attendance_EnrollmentSession").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.RecordedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ClassSession).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Sessions");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Enrollments");
        });

        modelBuilder.Entity<ClassSession>(entity =>
        {
            entity.HasIndex(e => new { e.CourseSectionId, e.StartAt }, "IX_ClassSessions_SectionTime");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.EndAt).HasPrecision(0);
            entity.Property(e => e.Room).HasMaxLength(50);
            entity.Property(e => e.StartAt).HasPrecision(0);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Scheduled");
            entity.Property(e => e.Topic).HasMaxLength(255);

            entity.HasOne(d => d.CourseSection).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.CourseSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassSessions_Sections");
        });

        modelBuilder.Entity<Club>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<ClubMember>(entity =>
        {
            entity.HasIndex(e => e.StudentId, "IX_ClubMembers_StudentId");

            entity.HasIndex(e => new { e.ClubId, e.StudentId }, "UQ_ClubMembers_ClubStudent").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.JoinedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Club).WithMany(p => p.ClubMembers)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClubMembers_Clubs");

            entity.HasOne(d => d.Student).WithMany(p => p.ClubMembers)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClubMembers_Students");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.CourseCode, "UQ_Courses_Code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_CourseSections_CourseId");

            entity.HasIndex(e => e.LecturerId, "IX_CourseSections_LecturerId");

            entity.HasIndex(e => new { e.SemesterId, e.SectionCode }, "UQ_CourseSections_SemesterSection").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.SectionCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Open");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseSections_Courses");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.LecturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseSections_Lecturers");

            entity.HasOne(d => d.Semester).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseSections_Semesters");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => e.CourseSectionId, "IX_Enrollments_SectionId");

            entity.HasIndex(e => new { e.StudentId, e.CourseSectionId }, "UQ_Enrollments_StudentSection").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.EnrolledAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Registered");

            entity.HasOne(d => d.CourseSection).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_CourseSections");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Students");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.EndAt).HasPrecision(0);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.RegistrationDeadline).HasPrecision(0);
            entity.Property(e => e.StartAt).HasPrecision(0);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.HasIndex(e => e.StudentId, "IX_EventRegistrations_StudentId");

            entity.HasIndex(e => new { e.EventId, e.StudentId }, "UQ_EventRegistrations_EventStudent").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.RegisteredAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Registered");

            entity.HasOne(d => d.Event).WithMany(p => p.EventRegistrations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventRegistrations_Events");

            entity.HasOne(d => d.Student).WithMany(p => p.EventRegistrations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventRegistrations_Students");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasIndex(e => e.GradeComponentId, "IX_Grades_ComponentId");

            entity.HasIndex(e => new { e.EnrollmentId, e.GradeComponentId }, "UQ_Grades_EnrollmentComponent").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.GradedAt).HasPrecision(0);
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Grades)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grades_Enrollments");

            entity.HasOne(d => d.GradeComponent).WithMany(p => p.Grades)
                .HasForeignKey(d => d.GradeComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grades_Components");
        });

        modelBuilder.Entity<GradeComponent>(entity =>
        {
            entity.HasIndex(e => e.CourseSectionId, "IX_GradeComponents_SectionId");

            entity.HasIndex(e => new { e.CourseSectionId, e.Name }, "UQ_GradeComponents_SectionName").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.MaxScore)
                .HasDefaultValue(10m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.CourseSection).WithMany(p => p.GradeComponents)
                .HasForeignKey(d => d.CourseSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GradeComponents_Sections");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasIndex(e => e.LecturerCode, "UQ_Lecturers_Code").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_Lecturers_UserId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.LecturerCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.Lecturer)
                .HasForeignKey<Lecturer>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lecturers_Users");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasIndex(e => e.MajorCode, "UQ_Majors_MajorCode").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MajorCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MajorName).HasMaxLength(200);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_Notifications_UserDate").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_PasswordResetTokens_UserId");

            entity.HasIndex(e => e.TokenHash, "UQ_PasswordResetTokens_TokenHash").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UsedAt).HasPrecision(0);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordResetTokens_Users");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "IX_RefreshTokens_UserId").IsDescending(false, true);

            entity.HasIndex(e => e.TokenHash, "UQ_RefreshTokens_TokenHash").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.RevokedAt).HasPrecision(0);
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.ReplacedByToken).WithMany(p => p.InverseReplacedByToken)
                .HasForeignKey(d => d.ReplacedByTokenId)
                .HasConstraintName("FK_RefreshTokens_ReplacedBy");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.SubmittedAt }, "IX_Requests_StudentDate").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.RequestType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Response).HasMaxLength(1000);
            entity.Property(e => e.ReviewedAt).HasPrecision(0);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SubmittedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_Requests_Reviewer");

            entity.HasOne(d => d.Student).WithMany(p => p.Requests)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requests_Students");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasIndex(e => new { e.AcademicYear, e.Name }, "UQ_Semesters_YearName").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.AcademicYear)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.MajorId, "IX_Students_MajorId");

            entity.HasIndex(e => e.StudentCode, "UQ_Students_Code").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_Students_UserId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Studying");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Major).WithMany(p => p.Students)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Majors");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
