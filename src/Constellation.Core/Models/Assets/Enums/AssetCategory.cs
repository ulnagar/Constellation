namespace Constellation.Core.Models.Assets.Enums;

using Core.Common;

public sealed class AssetCategory : StringEnumeration<AssetCategory>
{
    public static readonly AssetCategory Student = new("Student");
    public static readonly AssetCategory Staff = new("Staff");

    private AssetCategory(string value)
        : base(value, value) { }

    public static IEnumerable<AssetCategory> GetOptions => GetEnumerable;
}