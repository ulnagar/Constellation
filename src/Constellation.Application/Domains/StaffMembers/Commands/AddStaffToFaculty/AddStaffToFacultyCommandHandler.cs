namespace Constellation.Application.Domains.StaffMembers.Commands.AddStaffToFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStaffToFacultyCommandHandler
    : ICommandHandler<AddStaffToFacultyCommand>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddStaffToFacultyCommandHandler(
        IFacultyRepository facultyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddStaffToFacultyCommand>();
    }

    public async Task<Result> Handle(AddStaffToFacultyCommand request, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(request.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(AddStaffToFacultyCommandHandler), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(request.FacultyId), true)
                .Warning("Failed to add staff member to faculty");

            return Result.Failure(FacultyErrors.NotFound(request.FacultyId));
        }

        Result result = faculty.AddMember(request.StaffId, request.Role);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddStaffToFacultyCommandHandler), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add staff member to faculty");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
