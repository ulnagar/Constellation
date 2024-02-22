namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Application.Features.Equipment.Stocktake.Models;
using Constellation.Application.Interfaces.Repositories;
using Core.Models;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolsForSelectionQueryHandler 
    : IRequestHandler<GetSchoolsForSelectionQuery, ICollection<PartnerSchoolForDropdownSelection>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetSchoolsForSelectionQueryHandler(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<ICollection<PartnerSchoolForDropdownSelection>> Handle(GetSchoolsForSelectionQuery request, CancellationToken cancellationToken)
    {
        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        List<PartnerSchoolForDropdownSelection> result = schools.Select(school => 
                new PartnerSchoolForDropdownSelection()
                {
                    Code = school.Code, 
                    Name = school.Name
                })
            .ToList();

        return result;
    }
}