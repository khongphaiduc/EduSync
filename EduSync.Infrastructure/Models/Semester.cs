using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Semester
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
}
