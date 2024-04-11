namespace Constellation.Application.WorkFlows.AddSendEmailAction;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Enums;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Models;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSendEmailActionCommandHandler
: ICommandHandler<AddSendEmailActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddSendEmailActionCommandHandler(
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
        _logger = logger.ForContext<AddSendEmailActionCommand>();
    }

    public async Task<Result> Handle(AddSendEmailActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);
        if (item is null)
        {
            _logger
                .ForContext(nameof(AddSendEmailActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.Case.NotFound(request.CaseId), true)
                .Warning("Could not add Send Email Action to Case");

            return Result.Failure(CaseErrors.Case.NotFound(request.CaseId));
        }

        if (!item.Type!.Equals(CaseType.Attendance))
            return Result.Failure(CaseErrors.Action.Create.CaseTypeMismatch(CaseType.Attendance.Value, item.Type.Value));

        Staff teacher = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (teacher is null)
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));

        Result<SendEmailAction> action = SendEmailAction.Create(item.Id, teacher, _currentUserService.UserName);

        if (action.IsFailure)
        {
            _logger
                .ForContext(nameof(AddSendEmailActionCommand), request, true)
                .ForContext(nameof(Error), action.Error, true)
                .Warning("Could not add Send Email Action to Case");

            return Result.Failure(action.Error);
        }

        item.AddAction(action.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
