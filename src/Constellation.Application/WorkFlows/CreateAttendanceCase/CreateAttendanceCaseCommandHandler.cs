namespace Constellation.Application.WorkFlows.CreateAttendanceCase;

using Abstractions.Messaging;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Services;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAttendanceCaseCommandHandler
: ICommandHandler<CreateAttendanceCaseCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ICaseService _caseService;
    private readonly ICaseRepository _caseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAttendanceCaseCommandHandler(
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository,
        ICaseService caseService,
        ICaseRepository caseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
        _caseService = caseService;
        _caseRepository = caseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateAttendanceCaseCommand>();
    }

    public async Task<Result> Handle(CreateAttendanceCaseCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateAttendanceCaseCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        AttendanceValue attendanceValue = await _attendanceRepository.GetLatestForStudent(student.Id, cancellationToken);

        if (attendanceValue is null)
        {
            _logger
                .ForContext(nameof(CreateAttendanceCaseCommand), request, true)
                .ForContext(nameof(Error), AttendanceValueErrors.NotFoundForStudent(student.Id), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(AttendanceValueErrors.NotFoundForStudent(student.Id));
        }

        Result<Case> caseResult = await _caseService.CreateAttendanceCase(request.StudentId, attendanceValue.Id, cancellationToken);

        if (caseResult.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateAttendanceCaseCommand), request, true)
                .ForContext(nameof(Error), caseResult.Error, true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(caseResult.Error);
        }
        
        _caseRepository.Insert(caseResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
