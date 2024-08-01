namespace Constellation.Application.WorkFlows.Events.CaseActionCompletedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Errors;
using Constellation.Core.Models.WorkFlow.Events;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.WorkFlow.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddConfirmSentralActionForSentralIncidentAction
    : IDomainEventHandler<CaseActionCompletedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseRepository _courseRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddConfirmSentralActionForSentralIncidentAction(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        ICourseRepository courseRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _courseRepository = courseRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CaseActionCompletedDomainEvent>();
    }

    public async Task Handle(CaseActionCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(notification.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(notification.CaseId), true)
                .Warning("Could not create confirm Action for completed Sentral Incident action");

            return;
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == notification.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .ForContext(nameof(Error), ActionErrors.NotFound(notification.ActionId), true)
                .Warning("Could not create confirm Action for completed Sentral Incident action");

            return;
        }

        List<Staff> headTeachers = new();

        if (action is CreateSentralEntryAction sentralAction)
        {
            Course course = await _courseRepository.GetByOfferingId(sentralAction!.OfferingId!.Value, cancellationToken);

            if (course is null)
            {
                _logger
                    .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                    .ForContext(nameof(Error), ActionErrors.NotFound(notification.ActionId), true)
                    .Warning("Could not create confirm Action for completed Sentral Incident action");

                return;
            }

            headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(sentralAction!.OfferingId!.Value, cancellationToken);
        } 
        else if (action is SentralIncidentStatusAction)
        {
            var detail = item.Type!.Equals(CaseType.Compliance)
                ? item.Detail as ComplianceCaseDetail
                : null;

            if (detail is null)
                return;

            List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(detail?.CreatedById, cancellationToken);

            foreach (Faculty faculty in faculties)
            {
                if (faculty.Name is "Administration" or "Executive" or "Support")
                    continue;
                
                headTeachers.AddRange(await _staffRepository.GetFacultyHeadTeachers(faculty.Id, cancellationToken));
            }

            headTeachers = headTeachers.DistinctBy(entry => entry.StaffId).ToList();
        }
        else
        {
            return;
        }

        if (headTeachers.Count == 0)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .Warning("Could not create confirm Action for completed Sentral Incident action");

            return;
        }

        Result<ConfirmSentralEntryAction> confirmAction = ConfirmSentralEntryAction.Create(action.Id, item.Id, headTeachers.First(), _currentUserService.UserName);

        if (confirmAction.IsFailure)
        {
            _logger
                .ForContext(nameof(CaseActionCompletedDomainEvent), notification, true)
                .ForContext(nameof(Error), confirmAction.Error, true)
                .Warning("Could not create confirm Action for completed Sentral Incident action");

            return;
        }

        item.AddAction(confirmAction.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
