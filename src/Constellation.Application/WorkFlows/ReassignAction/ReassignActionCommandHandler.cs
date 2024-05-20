namespace Constellation.Application.WorkFlows.ReassignAction;

using Abstractions.Messaging;
using Constellation.Core.Models.WorkFlow.Errors;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReassignActionCommandHandler
: ICommandHandler<ReassignActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReassignActionCommandHandler(
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
        _logger = logger;
    }

    public async Task<Result> Handle(ReassignActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(ReassignActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Failed to reassign Action");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(ReassignActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), ActionErrors.NotFound(request.ActionId), true)
                .Warning("Failed to reassign Action");

            return Result.Failure(ActionErrors.NotFound(request.ActionId));
        }

        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(ReassignActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Action), action, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to reassign Action");

            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        Result assignRequest = item.ReassignAction(action.Id, staffMember, _currentUserService.UserName);

        if (assignRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(ReassignActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Action), action, true)
                .ForContext(nameof(Staff), staffMember, true)
                .ForContext(nameof(Error), assignRequest.Error, true)
                .Warning("Failed to reassign Action");

            return Result.Failure(assignRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
