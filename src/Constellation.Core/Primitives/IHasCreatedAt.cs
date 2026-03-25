namespace Constellation.Core.Primitives;

public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; }
}