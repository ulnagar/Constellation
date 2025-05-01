namespace Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolCodes;

using Abstractions.Messaging;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentPartnerSchoolCodesQueryHandler 
    : IQueryHandler<GetCurrentPartnerSchoolCodesQuery, List<string>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetCurrentPartnerSchoolCodesQueryHandler(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<string>>> Handle(GetCurrentPartnerSchoolCodesQuery request, CancellationToken cancellationToken)
    {
        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        return schools
            .Select(school => school.Code)
            .ToList();
    }
}