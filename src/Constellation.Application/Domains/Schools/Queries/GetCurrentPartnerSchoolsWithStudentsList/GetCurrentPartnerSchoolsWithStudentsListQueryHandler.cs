namespace Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;

using Abstractions.Messaging;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentPartnerSchoolsWithStudentsListQueryHandler
    : IQueryHandler<GetCurrentPartnerSchoolsWithStudentsListQuery, List<SchoolSelectionListResponse>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetCurrentPartnerSchoolsWithStudentsListQueryHandler(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<SchoolSelectionListResponse>>> Handle(GetCurrentPartnerSchoolsWithStudentsListQuery request, CancellationToken cancellationToken)
    {
        List<School> schools = await _schoolRepository.GetWithCurrentStudents(cancellationToken);

        return schools.OrderBy(school => school.Name).Select(school => new SchoolSelectionListResponse(school.Code, school.Name)).ToList();
    }
}
