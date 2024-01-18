namespace Constellation.Core.Models.Operations.Enums;

using Common;

public sealed class CanvasAction : StringEnumeration<CanvasAction>
{
    public static readonly CanvasAction Add = new("Add", "Add");
    public static readonly CanvasAction Remove = new("Remove", "Remove");

    private CanvasAction(string value, string name)
        : base(value, name) { }
}