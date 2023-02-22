namespace Constellation.Application.Schools.GetSchoolsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolsForSelectionListQueryHandler
    : IQueryHandler<GetSchoolsForSelectionListQuery, List<SchoolSelectionListResponse>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetSchoolsForSelectionListQueryHandler(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<SchoolSelectionListResponse>>> Handle(GetSchoolsForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<SchoolSelectionListResponse> returnData = new();

        var schools = await _schoolRepository.GetAll(cancellationToken);

        if (schools.Count == 0)
        {
            return returnData;
        }

        returnData.AddRange(schools
            .Select(school => 
                new SchoolSelectionListResponse(
                    school.Code, 
                    school.Name))
            .ToList());

        return returnData;
    }
}
