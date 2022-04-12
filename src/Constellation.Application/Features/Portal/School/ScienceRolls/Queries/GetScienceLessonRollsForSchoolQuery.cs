using Constellation.Application.Features.Portal.School.ScienceRolls.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Queries
{
    public class GetScienceLessonRollsForSchoolQuery : IRequest<ICollection<ScienceLessonRollForList>>
    {
        public string SchoolCode { get; set; }
    }
}
