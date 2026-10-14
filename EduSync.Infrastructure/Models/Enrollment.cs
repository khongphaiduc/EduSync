using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Enrollment
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid CourseSectionId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual CourseSection CourseSection { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Student Student { get; set; } = null!;
}
