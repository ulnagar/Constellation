using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.MissedWork
{
    public class TeacherMissedWorkNotificationEmailViewModel : EmailLayoutBaseViewModel
    {
        public string OfferingName { get; set; }
        public DateTime AbsenceDate { get; set; }
        public ICollection<string> StudentList { get; set; }
        public string Link { get; set; }
        public bool IsCovered { get; set; }
    }
}
