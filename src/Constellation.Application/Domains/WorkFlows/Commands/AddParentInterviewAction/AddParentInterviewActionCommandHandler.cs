namespace Constellation.Application.Domains.WorkFlows.Commands.AddParentInterviewAction;

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

internal sealed class AddParentInterviewActionCommandHandler
:ICommandHandler<AddParentInterviewActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddParentInterviewActionCommandHandler(
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
        _logger = logger.ForContext<AddParentInterviewActionCommand>();
    }

    public async Task<Result> Handle(AddParentInterviewActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);
        if (item is null)
        {
            _logger
                .ForContext(nameof(AddParentInterviewActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Could not add Parent Interview Action to Case");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        if (!item.Type!.Equals(CaseType.Attendance))
            return Result.Failure(ActionErrors.CreateCaseTypeMismatch(CaseType.Attendance.Value, item.Type.Value));

        StaffMember teacher = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (teacher is null)
            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));

        Result<ParentInterviewAction> action = ParentInterviewAction.Create(item.Id, teacher, _currentUserService.UserName);

        if (action.IsFailure)
        {
            _logger
                .ForContext(nameof(AddParentInterviewActionCommand), request, true)
                .ForContext(nameof(Error), action.Error, true)
                .Warning("Could not add Parent Interview Action to Case");

            return Result.Failure(action.Error);
        }

        item.AddAction(action.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
