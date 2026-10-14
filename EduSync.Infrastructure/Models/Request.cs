using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Request
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public string RequestType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? Response { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }

    public virtual Student Student { get; set; } = null!;
}
