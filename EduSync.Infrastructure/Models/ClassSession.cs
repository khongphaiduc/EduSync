using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class ClassSession
{
    public Guid Id { get; set; }

    public Guid CourseSectionId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Room { get; set; }

    public string? Topic { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual CourseSection CourseSection { get; set; } = null!;
}
