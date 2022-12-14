using System.ComponentModel.DataAnnotations;

namespace EF7ColumnJSON.Entities.Common;

public abstract class Entity
{
    [Key]
    public int Id { get; set; }
}
