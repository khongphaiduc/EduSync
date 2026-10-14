using System;
using System.Collections.Generic;

namespace EduSync.Infrastructure.Models;

public partial class Club
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? ContactInfo { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ClubMember> ClubMembers { get; set; } = new List<ClubMember>();
}
