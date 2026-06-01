namespace Constellation.Core.Models.Assessments.Enums;

using Core.Common;
using System.Collections.Generic;

public sealed class UserCategory : StringEnumeration<UserCategory>
{
    public static readonly UserCategory Parent = new("Parent");
    public static readonly UserCategory Student = new("Student");
    public static readonly UserCategory Coordinator = new("Coordinator");

    public UserCategory(string value) 
        : base(value, value) { }

    public static IEnumerable<UserCategory> GetOptions => GetEnumerable;
}
