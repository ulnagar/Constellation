using Constellation.Application.DTOs.Awards;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Auth.Queries
{
    public class GetMonthlyAwardDataForDashboardQueryHandler : IRequestHandler<GetMonthlyAwardDataForDashboardQuery, ICollection<AwardCountByTypeByMonth>>
    {
        private readonly IAppDbContext _context;

        public GetMonthlyAwardDataForDashboardQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<AwardCountByTypeByMonth>> Handle(GetMonthlyAwardDataForDashboardQuery request, CancellationToken cancellationToken)
        {
            var awards = await _context.StudentAward
                .Where(award => award.AwardedOn.Date >= DateTime.Today.AddMonths(-request.Months))
                .ToListAsync(cancellationToken);

            var returnData = new List<AwardCountByTypeByMonth>();

            for (int i = 0; i < request.Months; i++)
            {
                var monthDate = DateTime.Today.AddMonths(-i);

                var month = monthDate.Month switch
                {
                    1 => "Jan",
                    2 => "Feb",
                    3 => "Mar",
                    4 => "Apr",
                    5 => "May",
                    6 => "Jun",
                    7 => "Jul",
                    8 => "Aug",
                    9 => "Sep",
                    10 => "Oct",
                    11 => "Nov",
                    12 => "Dec"
                };

                for (int j = 0; j < 4; j++)
                {
                    var awardType = j switch
                    {
                        0 => "Astra Award",
                        1 => "Stellar Award",
                        2 => "Galaxy Medal",
                        3 => "Aurora Universal Achiever"
                    };

                    var entry = new AwardCountByTypeByMonth
                    {
                        MonthName = month,
                        MonthSort = monthDate.ToString("yyMM"),
                        AwardType = awardType,
                        Count = awards.Count(award => award.Type == awardType && award.AwardedOn.Year == monthDate.Year && award.AwardedOn.Month == monthDate.Month)
                    };

                    returnData.Add(entry);
                }   
            }

            return returnData;
        }
    }
}
