namespace Constellation.Application.Domains.Students.Commands.UpdateStudentAbsenceConfiguration;

using Abstractions.Messaging;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class UpdateStudentAbsenceConfigurationCommandHandler
: ICommandHandler<UpdateStudentAbsenceConfigurationCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStudentAbsenceConfigurationCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork  unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateStudentAbsenceConfigurationCommand>();
    }

    public async Task<Result> Handle(UpdateStudentAbsenceConfigurationCommand request, CancellationToken cancellationToken)
    {
        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(UpdateStudentAbsenceConfigurationCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to update Student Absence Configuration");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        AbsenceConfiguration? configuration = student.AbsenceConfigurations
            .FirstOrDefault(entry =>
                entry.AbsenceType == request.Type
                && request.EndDate >= entry.ScanStartDate
                && request.EndDate <= entry.ScanEndDate
                && !entry.IsDeleted);

        if (configuration is null)
        {
            _logger
                .ForContext(nameof(UpdateStudentAbsenceConfigurationCommand), request, true)
                .ForContext(nameof(AbsenceConfiguration), student.AbsenceConfigurations, true)
                .ForContext(nameof(Error), AbsenceConfigurationErrors.NoMatchFound, true)
                .Warning("Failed to update Student Absence Configuration");

            return Result.Failure(AbsenceConfigurationErrors.NoMatchFound);
        }

        Result result = configuration.Cancel(request.EndDate);

        if (result.IsFailure)
            return result;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
