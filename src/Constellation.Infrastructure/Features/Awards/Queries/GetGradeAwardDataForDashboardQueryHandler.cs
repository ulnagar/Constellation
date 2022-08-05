using Constellation.Application.DTOs.Awards;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Awards.Queries
{
    public class GetGradeAwardDataForDashboardQueryHandler : IRequestHandler<GetGradeAwardDataForDashboardQuery, ICollection<AwardCountByTypeByGrade>>
    {
        private readonly IAppDbContext _context;

        public GetGradeAwardDataForDashboardQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<AwardCountByTypeByGrade>> Handle(GetGradeAwardDataForDashboardQuery request, CancellationToken cancellationToken)
        {
            var awards = await _context.StudentAward
                .Include(award => award.Student)
                .Where(award => award.AwardedOn.Year == request.Year)
                .ToListAsync(cancellationToken);

            var returnData = new List<AwardCountByTypeByGrade>();

            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
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

                    var entry = new AwardCountByTypeByGrade
                    {
                        ReportPeriod = "YTD",
                        Grade = grade.AsName(),
                        AwardType = awardType,
                        Count = awards.Count(award => award.Type == awardType && award.Student.CurrentGrade == grade)
                    };

                    returnData.Add(entry);
                }
            }

            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
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

                    var entry = new AwardCountByTypeByGrade
                    {
                        ReportPeriod = "This Month",
                        Grade = grade.AsName(),
                        AwardType = awardType,
                        Count = awards.Count(award => award.Type == awardType && award.Student.CurrentGrade == grade && award.AwardedOn.Month == DateTime.Today.Month)
                    };

                    returnData.Add(entry);
                }
            }

            return returnData;
        }
    }
}
