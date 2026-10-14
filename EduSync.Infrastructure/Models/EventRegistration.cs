using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class EventRegistration
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
