namespace Constellation.Core.Primitives;

using Constellation.Core.Enums;

public abstract class SystemLink
{
    private SystemLink() { }

    protected SystemLink(
        SystemType system,
        string value)
    {
        System = system;
        Value = value;
    }

    public SystemType System { get; protected set; }
    public string Value { get; protected set; }
}