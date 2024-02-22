namespace Constellation.Application.Features.Portal.School.Home.Queries;

using Constellation.Application.Interfaces.Repositories;
using Core.Models;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllPartnerSchoolCodesQueryHandler 
    : IRequestHandler<GetAllPartnerSchoolCodesQuery, ICollection<string>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetAllPartnerSchoolCodesQueryHandler(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<ICollection<string>> Handle(GetAllPartnerSchoolCodesQuery request, CancellationToken cancellationToken)
    {
        List<School> schools =  await _schoolRepository.GetWithCurrentStudents(cancellationToken);

        return schools.Select(school => school.Code).ToList();
    }
}