namespace Constellation.Core.Primitives;

using System;

public interface IValueObject<TSelf, TValue>
    where TSelf : ValueObject<TSelf, TValue>
    where TValue : IEquatable<TValue>
{
    static abstract TSelf FromValue(TValue value);
}

public abstract class ValueObject<TSelf, TValue> : IEquatable<TSelf>
    where TSelf : ValueObject<TSelf, TValue>
    where TValue : IEquatable<TValue>
{
    public TValue Value { get; protected init; }

    public bool Equals(TSelf? other)
    {
        if (other is null) return false;
        if (other.Value is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => obj is TSelf other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString()!;

    public static bool operator ==(ValueObject<TSelf, TValue>? left, ValueObject<TSelf, TValue>? right)
        => left?.Equals(right as TSelf) ?? right is null;

    public static bool operator !=(ValueObject<TSelf, TValue>? left, ValueObject<TSelf, TValue>? right)
        => !(left == right);
}

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetAtomicValues();

    public bool Equals(ValueObject? other)
    {
        return other is not null && ValuesAreEqual(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && ValuesAreEqual(other);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(
                default(int),
                HashCode.Combine);
    }

    private bool ValuesAreEqual(ValueObject other)
    {
        return GetAtomicValues()
            .SequenceEqual(other.GetAtomicValues());
    }

    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (left is null ^ right is null)
            return false;

        if (left is null & right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.ValuesAreEqual(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return EqualOperator(left, right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return NotEqualOperator(left, right);
    }
}