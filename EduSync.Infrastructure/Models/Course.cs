using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Course
{
    public Guid Id { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public int Credits { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
}
