namespace Constellation.Application.WorkFlows.UpdateAttendanceCaseDetails;

using Abstractions.Messaging;
using Constellation.Application.WorkFlows.AddCaseDetailUpdateAction;
using Constellation.Core.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow.Enums;
using Core.Abstractions.Services;
using Core.Models;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAttendanceCaseDetailsCommandHandler
: ICommandHandler<UpdateAttendanceCaseDetailsCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAttendanceCaseDetailsCommandHandler(
        ICaseRepository caseRepository,
        IAttendanceRepository attendanceRepository,
        IStaffRepository staffRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _attendanceRepository = attendanceRepository;
        _staffRepository = staffRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateAttendanceCaseDetailsCommand>();
    }

    public async Task<Result> Handle(UpdateAttendanceCaseDetailsCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetOpenAttendanceCaseForStudent(request.StudentId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(UpdateAttendanceCaseDetailsCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.Case.NotFoundForStudent(request.StudentId), true)
                .Warning("Failed to update existing Case");

            return Result.Failure(CaseErrors.Case.NotFoundForStudent(request.StudentId));
        }

        if (!item.Type!.Equals(CaseType.Attendance))
            return Result.Failure(CaseErrors.Action.Create.CaseTypeMismatch(CaseType.Attendance.Value, item.Type.Value));

        AttendanceValue value = await _attendanceRepository.GetLatestForStudent(request.StudentId, cancellationToken);

        if (value is null)
        {
            _logger
                .ForContext(nameof(UpdateAttendanceCaseDetailsCommand), request, true)
                .ForContext(nameof(Error), AttendanceValueErrors.NotFoundForStudent(request.StudentId), true)
                .Warning("Failed to update existing Case");

            return Result.Failure(AttendanceValueErrors.NotFoundForStudent(request.StudentId));
        }

        Staff teacher = await _staffRepository.GetCurrentByEmailAddress(_currentUserService.EmailAddress, cancellationToken);

        if (teacher is null)
        {
            _logger
                .ForContext(nameof(UpdateAttendanceCaseDetailsCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress), true)
                .Warning("Failed to update existing Case");
            
            return Result.Failure(DomainErrors.Partners.Staff.NotFoundByEmail(_currentUserService.EmailAddress));
        }

        string details = $"Attendance percentage for period {value.PeriodLabel} is {value.PerMinuteWeekPercentage}";

        Result<CaseDetailUpdateAction> action = CaseDetailUpdateAction.Create(null, item.Id, teacher, details);

        if (action.IsFailure)
        {
            _logger
                .ForContext(nameof(AddCaseDetailUpdateActionCommand), request, true)
                .ForContext(nameof(Error), action.Error, true)
                .Warning("Could not add Case Detail Update Action to Case");

            return Result.Failure(action.Error);
        }

        item.AddAction(action.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
