namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsForSelectionList;

using Abstractions.Messaging;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using Models;
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

        List<School> schools = request.Filter switch
        {
            GetSchoolsForSelectionListQuery.SchoolsFilter.All => await _schoolRepository.GetAll(cancellationToken),
            GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools => await _schoolRepository.GetAllActive(cancellationToken),
            GetSchoolsForSelectionListQuery.SchoolsFilter.WithStudents => await _schoolRepository.GetWithCurrentStudents(cancellationToken),
            _ => await _schoolRepository.GetAll(cancellationToken)
        };

        if (schools.Count == 0)
            return returnData;

        returnData.AddRange(schools
            .Select(school => 
                new SchoolSelectionListResponse(
                    school.Code, 
                    school.Name))
            .ToList());

        return returnData;
    }
}
