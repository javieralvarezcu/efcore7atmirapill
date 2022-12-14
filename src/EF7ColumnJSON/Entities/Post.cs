using EF7ColumnJSON.Entities.Common;

namespace EF7ColumnJSON.Entities;

public class Post : Entity
{
    public string Title { get; set; }
    public string Body { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }
}