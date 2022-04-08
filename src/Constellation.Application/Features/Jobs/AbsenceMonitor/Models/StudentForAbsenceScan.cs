using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Models
{
    public class StudentForAbsenceScan : IMapFrom<Student>
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
        public Grade CurrentGrade { get; set; }
        public string SentralStudentId { get; set; }
        public DateTime? AbsenceNotificationStartDate { get; set; }
        public string PortalUsername { get; set; }
        public string EmailAddress => $"{PortalUsername}@education.nsw.gov.au";
        public string SchoolName { get; set; }
    }
}
