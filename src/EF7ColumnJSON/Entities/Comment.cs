using EF7ColumnJSON.Entities.Common;

namespace EF7ColumnJSON.Entities;

public class Comment : Entity
{
    public int PostId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Body { get; set; }
}