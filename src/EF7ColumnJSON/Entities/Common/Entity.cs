using System.ComponentModel.DataAnnotations;

namespace EF7ColumnJSON.Entities.Common;

public abstract class Entity
{
    [Key]
    public Guid Id { get; private set; } = new();
}
