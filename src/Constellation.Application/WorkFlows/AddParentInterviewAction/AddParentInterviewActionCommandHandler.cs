namespace Constellation.Application.WorkFlows.AddParentInterviewAction;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Errors;
using Core.Models;
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

        Staff teacher = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (teacher is null)
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));

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
