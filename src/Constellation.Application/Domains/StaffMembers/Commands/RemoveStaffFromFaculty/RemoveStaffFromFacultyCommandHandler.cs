﻿namespace Constellation.Application.Domains.StaffMembers.Commands.RemoveStaffFromFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStaffFromFacultyCommandHandler
    : ICommandHandler<RemoveStaffFromFacultyCommand>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStaffFromFacultyCommandHandler(
        IFacultyRepository facultyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveStaffFromFacultyCommand>();
    }

    public async Task<Result> Handle(RemoveStaffFromFacultyCommand request, CancellationToken cancellationToken)
    {
        Faculty faculty = await _facultyRepository.GetById(request.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(RemoveStaffFromFacultyCommand), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(request.FacultyId), true)
                .Warning("Failed to remove staff member from faculty");

            return Result.Failure(FacultyErrors.NotFound(request.FacultyId));
        }

        Result result = faculty.RemoveMember(request.StaffId);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveStaffFromFacultyCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove staff member from faculty");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
