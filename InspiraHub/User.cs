using System;
using System.Collections.Generic;

namespace InspiraHub;

public partial class User
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly DateBirth { get; set; }

    public string Password { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
}
