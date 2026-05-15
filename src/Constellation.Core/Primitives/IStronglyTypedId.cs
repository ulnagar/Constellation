namespace Constellation.Core.Primitives;

public interface IStronglyTypedId { }

public interface IStronglyTypedId<TSelf, TValue> : IStronglyTypedId
    where TSelf : IStronglyTypedId<TSelf, TValue>
{
    TValue Value { get; }

    static abstract TSelf FromValue(TValue value);
    static abstract TSelf Empty { get; }

    static TSelf Parse<TSelf, TValue>(TValue raw)
        where TSelf : IStronglyTypedId<TSelf, TValue>
        => TSelf.FromValue(raw);
}