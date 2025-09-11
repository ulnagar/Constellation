namespace Constellation.Core.Models.Tutorials.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class TutorialType : StringEnumeration<TutorialType>
{
    public static readonly TutorialType Unknown = new("");
    public static readonly TutorialType Subject = new("Subject Support");
    public static readonly TutorialType Study = new("Study Support");

    private TutorialType(string value)
        : base(value, value) { }

    public static IEnumerable<TutorialType> GetOptions => GetEnumerable;
}