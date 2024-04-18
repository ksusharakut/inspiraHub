using System;
using System.Collections.Generic;

namespace InspiraHub;

public partial class Content
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Preview { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreateAt { get; set; }

    public string ContentType { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User User { get; set; } = null!;
}
