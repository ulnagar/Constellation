namespace Constellation.Application.Faculties.GetFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFacultyQueryHandler
:IQueryHandler<GetFacultyQuery, FacultyResponse>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetFacultyQueryHandler(
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetFacultyQuery>();
    }

    public async Task<Result<FacultyResponse>> Handle(GetFacultyQuery request, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(request.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(GetFacultyQuery), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(request.FacultyId), true)
                .Warning("Failed to retrieve Faculty");

            return Result.Failure<FacultyResponse>(FacultyErrors.NotFound(request.FacultyId));
        }

        return new FacultyResponse(
            faculty.Id,
            faculty.Name,
            faculty.Colour);
    }

}
