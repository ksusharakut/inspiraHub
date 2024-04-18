using System;
using System.Collections.Generic;

namespace InspiraHub;

public partial class Comment
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long ContentId { get; set; }

    public string UserComment { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public string UserName { get; set; } = null!;

    public virtual Content Content { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
