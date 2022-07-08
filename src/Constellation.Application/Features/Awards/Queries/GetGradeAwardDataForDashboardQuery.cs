using Constellation.Application.DTOs.Awards;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Awards.Queries
{
    public class GetGradeAwardDataForDashboardQuery : IRequest<ICollection<AwardCountByTypeByGrade>>
    {
        public int Year { get; set; }
    }
}
