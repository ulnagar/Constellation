namespace Constellation.Application.Domains.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddUploadTrainingCertificationActionForTrainingCase
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddUploadTrainingCertificationActionForTrainingCase(
        ICaseRepository caseRepository,
        IStaffRepository staffRepository,
        ITrainingModuleRepository moduleRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _staffRepository = staffRepository;
        _moduleRepository = moduleRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddUploadTrainingCertificationActionForTrainingCase>();
    }

    public async Task Handle(CaseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        if (!item.Type!.Equals(CaseType.Training))
            return;

        TrainingCaseDetail caseDetail = item.Detail as TrainingCaseDetail;

        StaffMember assignee = await _staffRepository.GetById(caseDetail!.StaffId, cancellationToken);
        if (assignee is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(caseDetail!.StaffId), true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        TrainingModule module = await _moduleRepository.GetModuleById(caseDetail.TrainingModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(caseDetail.TrainingModuleId), true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        Result<UploadTrainingCertificateAction> uploadAction = UploadTrainingCertificateAction.Create(item.Id, module, assignee, _currentUserService.UserName);

        if (uploadAction.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), uploadAction.Error, true)
                .Warning("Could not create default Action for new Case");

            return;
        }

        item.AddAction(uploadAction.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}