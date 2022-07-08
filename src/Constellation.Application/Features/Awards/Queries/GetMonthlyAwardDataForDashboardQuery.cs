using Constellation.Application.DTOs.Awards;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Awards.Queries
{
    public class GetMonthlyAwardDataForDashboardQuery : IRequest<ICollection<AwardCountByTypeByMonth>>
    {
        public int Months { get; set; }
    }
}
