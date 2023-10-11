using EF7ColumnJSON.Entities.Common;

namespace EF7ColumnJSON.Entities;

public class User : Entity
{
    public string Name { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public Address Address { get; set; } 

public virtual ICollection<Post>? Posts { get; set; }
}