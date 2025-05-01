namespace Constellation.Application.Domains.WorkFlows.Commands.AddCaseDetailUpdateAction;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddCaseDetailUpdateActionCommandHandler
: ICommandHandler<AddCaseDetailUpdateActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddCaseDetailUpdateActionCommandHandler(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddCaseDetailUpdateActionCommand>();
    }

    public async Task<Result> Handle(AddCaseDetailUpdateActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);
        if (item is null)
        {
            _logger
                .ForContext(nameof(AddCaseDetailUpdateActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Could not add Case Detail Update Action to Case");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        Staff teacher = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (teacher is null)
            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));

        Result<CaseDetailUpdateAction> action = CaseDetailUpdateAction.Create(null, item.Id, teacher, request.Details);

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
