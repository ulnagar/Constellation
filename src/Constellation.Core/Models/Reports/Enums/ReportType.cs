namespace Constellation.Core.Models.Reports.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class ReportType : StringEnumeration<ReportType>
{
    public static readonly ReportType Unknown = new("");
    public static readonly ReportType PATM = new("PAT Mathematics Adaptive");
    public static readonly ReportType PATR = new("PAT Reading Adaptive");

    private ReportType(string value)
        : base(value, value) { }

    public static IEnumerable<ReportType> GetOptions => GetEnumerable;
}