using Constellation.Application.Features.Portal.School.Absences.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Absences.Queries
{
    public class GetUnProcessedAbsencesFromSchoolQuery : IRequest<ICollection<AbsenceForPortalList>>
    {
        public string SchoolCode { get; set; }
    }
}
