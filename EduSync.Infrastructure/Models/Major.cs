using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Major
{
    public Guid Id { get; set; }

    public string MajorCode { get; set; } = null!;

    public string MajorName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
