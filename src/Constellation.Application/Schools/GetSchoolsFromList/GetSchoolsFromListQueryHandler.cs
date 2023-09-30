namespace Constellation.Application.Schools.GetSchoolsFromList;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Core.Models;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolsFromListQueryHandler 
    : IQueryHandler<GetSchoolsFromListQuery, List<SchoolDto>>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetSchoolsFromListQueryHandler(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<SchoolDto>>> Handle(GetSchoolsFromListQuery request, CancellationToken cancellationToken)
    {
        List<SchoolDto> response = new();

        List<School> schools = await _schoolRepository.GetListFromIds(request.SchoolCodes, cancellationToken);

        foreach (School school in schools)
        {
            response.Add(new()
            {
                Code = school.Code,
                Name = school.Name
            });
        }

        return response;
    }
}