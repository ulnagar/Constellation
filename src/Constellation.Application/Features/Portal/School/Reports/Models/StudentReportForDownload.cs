using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;

namespace Constellation.Application.Features.Portal.School.Reports.Models
{
    public class StudentReportForDownload : IMapFrom<StudentReport>
    {
        public string StudentId { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public Grade StudentCurrentGrade { get; set; }
        public string StudentName => $"{StudentFirstName} {StudentLastName}";
        public string StudentGrade => $"Year {((int)StudentCurrentGrade):D2}";
        public string PublishId { get; set; }
        public Guid Id { get; set; }
        public string Year { get; set; }
        public string ReportingPeriod { get; set; }
    }
}
