namespace Constellation.Application.Domains.Casuals.Queries.GetInactiveCasuals;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetInactiveCasualsQueryHandler
    : IQueryHandler<GetInactiveCasualsQuery, List<CasualsListResponse>>
{
    private readonly ICasualRepository _casualRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetInactiveCasualsQueryHandler(
        ICasualRepository casualRepository,
        ISchoolRepository schoolRepository)
    {
        _casualRepository = casualRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<CasualsListResponse>>> Handle(GetInactiveCasualsQuery request, CancellationToken cancellationToken)
    {
        var returnData = new List<CasualsListResponse>();

        var casuals = await _casualRepository.GetAllInactive(cancellationToken);

        if (casuals.Count == 0)
        {
            return returnData;
        }

        foreach (var casual in casuals)
        {
            string schoolName = string.Empty;

            if (!string.IsNullOrWhiteSpace(casual.SchoolCode))
            {
                var school = await _schoolRepository.GetById(casual.SchoolCode, cancellationToken);
                if (school is not null)
                {
                    schoolName = school.Name;
                }
            }

            var entry = new CasualsListResponse(
                casual.Id,
                casual.Name,
                schoolName,
                casual.EmailAddress.Email,
                casual.EdvalTeacherId,
                !casual.IsDeleted);

            returnData.Add(entry);
        }

        return returnData;
    }
}
