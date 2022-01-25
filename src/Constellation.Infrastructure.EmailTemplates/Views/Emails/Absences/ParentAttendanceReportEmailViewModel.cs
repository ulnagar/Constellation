using Constellation.Infrastructure.Templates.Views.Shared;
using System;

namespace Constellation.Infrastructure.Templates.Views.Emails.Absences
{
    public class ParentAttendanceReportEmailViewModel : EmailLayoutBaseViewModel
    {
        public string StudentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
