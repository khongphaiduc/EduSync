using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class GradeComponent
{
    public Guid Id { get; set; }

    public Guid CourseSectionId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Weight { get; set; }

    public decimal MaxScore { get; set; }

    public virtual CourseSection CourseSection { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
