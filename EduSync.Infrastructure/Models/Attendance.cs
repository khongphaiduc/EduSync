using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Attendance
{
    public Guid Id { get; set; }

    public Guid EnrollmentId { get; set; }

    public Guid ClassSessionId { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual ClassSession ClassSession { get; set; } = null!;

    public virtual Enrollment Enrollment { get; set; } = null!;
}
