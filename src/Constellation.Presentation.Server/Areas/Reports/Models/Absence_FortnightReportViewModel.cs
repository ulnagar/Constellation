using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Absence_FortnightReportViewModel
    {
        public Absence_FortnightReportViewModel()
        {
            Absences = new List<Absence>();
            DateData = new List<DateSessions>();
            ExcludedDates = new List<DateTime>();
        }

        public Student Student { get; set; }
        public DateTime StartDate { get; set; }
        public ICollection<DateSessions> DateData { get; set; }
        public ICollection<Absence> Absences { get; set; }
        public ICollection<DateTime> ExcludedDates { get; set; }

        public class DateSessions
        {
            public DateSessions()
            {
                SessionsByOffering = new List<IGrouping<int, OfferingSession>>();
            }

            public DateTime Date { get; set; }
            public int DayNumber { get; set; }
            public ICollection<IGrouping<int, OfferingSession>> SessionsByOffering { get; set; }
        }
    }
}