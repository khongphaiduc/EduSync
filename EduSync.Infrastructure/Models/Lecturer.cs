using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Lecturer
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string LecturerCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public virtual User User { get; set; } = null!;
}
