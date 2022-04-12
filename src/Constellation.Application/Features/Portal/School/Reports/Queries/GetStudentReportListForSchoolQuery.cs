using Constellation.Application.Features.Portal.School.Reports.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Reports.Queries
{
    public class GetStudentReportListForSchoolQuery : IRequest<ICollection<StudentReportForDownload>>
    {
        public string SchoolCode { get; set; }
    }
}
