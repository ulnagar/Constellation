namespace Constellation.Application.Faculties.GetFacultiesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFacultiesForSelectionListQueryHandler
    : IQueryHandler<GetFacultiesForSelectionListQuery, List<FacultySummaryResponse>>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetFacultiesForSelectionListQueryHandler(
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetFacultiesForSelectionListQuery>();
    }

    public async Task<Result<List<FacultySummaryResponse>>> Handle(GetFacultiesForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<FacultySummaryResponse> response = new();

        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        foreach (Faculty faculty in faculties)
        {
            response.Add(new(
                faculty.Id,
                faculty.Name));
        }

        return response;
    }
}
