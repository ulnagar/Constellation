using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.SentralReportSync.Models
{
    public class StudentForReportDownload : IMapFrom<Student>
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}";
        public Grade CurrentGrade { get; set; }
        public string Grade => $"Year {((int)CurrentGrade):D2}";
        public string SentralStudentId { get; set; }

        public ICollection<Report> Reports { get; set; }

        public class Report : IMapFrom<StudentReport>
        {
            public string StudentId { get; set; }
            public string PublishId { get; set; }
            public string Year { get; set; }
            public string ReportingPeriod { get; set; }
        }
    }
}
