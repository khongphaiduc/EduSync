using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class CourseSection
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public Guid SemesterId { get; set; }

    public Guid LecturerId { get; set; }

    public string SectionCode { get; set; } = null!;

    public int Capacity { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<GradeComponent> GradeComponents { get; set; } = new List<GradeComponent>();

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual Semester Semester { get; set; } = null!;
}
