using Constellation.Infrastructure.Templates.Views.Shared;
using System;

namespace Constellation.Infrastructure.Templates.Views.Emails.MissedWork
{
    public class StudentMissedWorkNotificationEmailViewModel : EmailLayoutBaseViewModel
    {
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string OfferingName { get; set; }
        public DateTime AbsenceDate { get; set; }
        public string WorkDescription { get; set; }
    }
}
