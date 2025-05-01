namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardCountsByTypeByMonth;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardCountsByTypeByMonthQueryHandler
    : IQueryHandler<GetAwardCountsByTypeByMonthQuery, List<AwardCountByTypeByMonthResponse>>
{
    private readonly IStudentAwardRepository _awardRepository;

    public GetAwardCountsByTypeByMonthQueryHandler(
        IStudentAwardRepository awardRepository)
    {
        _awardRepository = awardRepository;
    }

    public async Task<Result<List<AwardCountByTypeByMonthResponse>>> Handle(GetAwardCountsByTypeByMonthQuery request, CancellationToken cancellationToken)
    {
        List<AwardCountByTypeByMonthResponse> result = new();

        var awards = await _awardRepository.GetFromRecentMonths(request.Months, cancellationToken);

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
                12 => "Dec",
                _ => ""
            };

            for (int j = 0; j <= 3; j++)
            {
                var awardType = j switch
                {
                    0 => "Astra Award",
                    1 => "Stellar Award",
                    2 => "Galaxy Medal",
                    3 => "Aurora Universal Achiever",
                    _ => ""
                };

                var entry = new AwardCountByTypeByMonthResponse(
                    month,
                    monthDate.ToString("yyMM"),
                    awardType,
                    awards.Count(award => award.Type == awardType && award.AwardedOn.Year == monthDate.Year && award.AwardedOn.Month == monthDate.Month));

                result.Add(entry);
            }
        }

        return result;
    }
}
