namespace Constellation.Application.Features.API.Schools.Queries;

using Core.Models;
using Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolCodeOfAllPartnerSchoolsQueryHandler : IRequestHandler<GetSchoolCodeOfAllPartnerSchoolsQuery, ICollection<string>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetSchoolCodeOfAllPartnerSchoolsQueryHandler(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<ICollection<string>> Handle(GetSchoolCodeOfAllPartnerSchoolsQuery request, CancellationToken cancellationToken)
    {
        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        return schools.Select(school => school.Code).ToList();
    }
}