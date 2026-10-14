using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class ClubMember
{
    public Guid Id { get; set; }

    public Guid ClubId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Club Club { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
