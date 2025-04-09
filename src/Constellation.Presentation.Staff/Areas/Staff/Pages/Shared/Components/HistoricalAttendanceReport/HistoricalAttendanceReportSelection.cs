namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.HistoricalAttendanceReport;

using Constellation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class HistoricalAttendanceReportSelection
{
    public string Year { get; set; }
    public SchoolTerm StartTerm { get; set; }
    public SchoolWeek StartWeek { get; set; }
    public SchoolTerm EndTerm { get; set; }
    public SchoolWeek EndWeek { get; set; }

    public List<string> Years { get; set; }
    public IEnumerable<SchoolTerm> SchoolTerms => SchoolTerm.GetOptions;
    public IEnumerable<SchoolWeek> SchoolWeeks => SchoolWeek.GetOptions;
}
