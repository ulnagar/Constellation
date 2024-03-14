namespace Constellation.Core.Models.WorkFlow.Enums;

using Constellation.Core.Common;

public class CaseType : StringEnumeration<CaseType>
{
    public static readonly CaseType Attendance = new("Attendance", "Attendance");

    private CaseType(string value, string name)
        : base(value, name)
    { }
}