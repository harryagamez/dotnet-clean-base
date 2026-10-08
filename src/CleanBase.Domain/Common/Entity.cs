namespace CleanBase.Domain.Common;

/// <summary>Base class for objects with identity.</summary>
public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>Parameterless constructor for EF Core materialization.</summary>
    protected Entity()
    {
    }

    public Guid Id { get; private init; }
}
