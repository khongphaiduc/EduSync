using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Student
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public Guid MajorId { get; set; }

    public int EnrollmentYear { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<ClubMember> ClubMembers { get; set; } = new List<ClubMember>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual User User { get; set; } = null!;
}
