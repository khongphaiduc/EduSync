using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Grade
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }

    public Guid GradeComponentId { get; set; }

    public decimal? Score { get; set; }

    public DateTime? GradedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual GradeComponent GradeComponent { get; set; } = null!;
}
