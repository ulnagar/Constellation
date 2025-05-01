namespace Constellation.Application.Domains.Faculties.Commands.UpdateFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateFacultyCommandHandler 
    :ICommandHandler<UpdateFacultyCommand>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateFacultyCommandHandler(
        IFacultyRepository facultyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateFacultyCommand>();
    }

    public async Task<Result> Handle(UpdateFacultyCommand request, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository
            .GetById(request.Id, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(UpdateFacultyCommand), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(request.Id), true)
                .Warning("Failed to update faculty details");

            return Result.Failure(FacultyErrors.NotFound(request.Id));
        }

        faculty.Update(
            request.Name,
            request.Colour);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}