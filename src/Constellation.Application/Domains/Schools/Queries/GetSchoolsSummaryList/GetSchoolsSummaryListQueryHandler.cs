namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsSummaryList;

using Abstractions.Messaging;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolsSummaryListQueryHandler
: IQueryHandler<GetSchoolsSummaryListQuery, List<SchoolSummaryResponse>>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetSchoolsSummaryListQueryHandler(
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetSchoolsSummaryListQuery>();
    }

    public async Task<Result<List<SchoolSummaryResponse>>> Handle(GetSchoolsSummaryListQuery request, CancellationToken cancellationToken)
    {
        List<SchoolSummaryResponse> response = new();

        List<School> schools = request.Filter switch
        {
            SchoolFilter.All => await _schoolRepository.GetAll(cancellationToken),
            SchoolFilter.Inactive => await _schoolRepository.GetAllInactive(cancellationToken),
            _ => await _schoolRepository.GetAllActive(cancellationToken)
        };

        foreach (School school in schools)
        {
            response.Add(new(
                school.Code,
                school.Name,
                school.Town,
                school.PhoneNumber,
                school.EmailAddress));
        }

        return response;
    }
}
