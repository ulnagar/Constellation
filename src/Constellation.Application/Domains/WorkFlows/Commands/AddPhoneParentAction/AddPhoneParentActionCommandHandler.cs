namespace Constellation.Application.Domains.WorkFlows.Commands.AddPhoneParentAction;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddPhoneParentActionCommandHandler
: ICommandHandler<AddPhoneParentActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddPhoneParentActionCommandHandler(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddPhoneParentActionCommand>();
    }

    public async Task<Result> Handle(AddPhoneParentActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);
        if (item is null)
        {
            _logger
                .ForContext(nameof(AddPhoneParentActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Could not add Phone Parent Action to Case");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        if (!item.Type!.Equals(CaseType.Attendance))
            return Result.Failure(ActionErrors.CreateCaseTypeMismatch(CaseType.Attendance.Value, item.Type.Value));

        StaffMember teacher = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (teacher is null)
            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));

        Result<PhoneParentAction> action = PhoneParentAction.Create(item.Id, teacher, _currentUserService.UserName);

        if (action.IsFailure)
        {
            _logger
                .ForContext(nameof(AddPhoneParentActionCommand), request, true)
                .ForContext(nameof(Error), action.Error, true)
                .Warning("Could not add Phone Parent Action to Case");

            return Result.Failure(action.Error);
        }

        item.AddAction(action.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
