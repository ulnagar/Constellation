namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesSummary;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetFacultiesSummaryQueryHandler 
    : IQueryHandler<GetFacultiesSummaryQuery, List<FacultySummaryResponse>>
{
    private readonly IFacultyRepository _facultyRepository;

    public GetFacultiesSummaryQueryHandler(
        IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<List<FacultySummaryResponse>>> Handle(GetFacultiesSummaryQuery request, CancellationToken cancellationToken)
    {
        List<FacultySummaryResponse> response = new();
        
        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        foreach (Faculty faculty in faculties)
        {
            response.Add(new(
                faculty.Id,
                faculty.Name,
                faculty.Colour,
                faculty.MemberCount));
        }

        return response;
    }
}