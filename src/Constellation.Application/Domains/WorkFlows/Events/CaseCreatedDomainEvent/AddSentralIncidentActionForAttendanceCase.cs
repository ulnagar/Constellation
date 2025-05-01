namespace Constellation.Application.Domains.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Events;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Configuration;
using Interfaces.Repositories;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSentralIncidentActionForAttendanceCase
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;

    public AddSentralIncidentActionForAttendanceCase(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        IOptions<AppConfiguration> configuration,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration.Value;
        _logger = logger.ForContext<CaseCreatedDomainEvent>();
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

        if (!item.Type!.Equals(CaseType.Attendance))
            return;

        AttendanceCaseDetail caseDetail = item.Detail as AttendanceCaseDetail;

        List<AttendanceSeverity> severityList = new()
        {
            AttendanceSeverity.BandTwo,
            AttendanceSeverity.BandThree,
            AttendanceSeverity.BandFour
        };

        if (!severityList.Contains(caseDetail!.Severity))
            return;

        Staff reviewer = await _staffRepository.GetById(_configuration.WorkFlow.AttendanceReviewer, cancellationToken);
        if (reviewer is null)
        {
            _logger
                .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(_configuration.WorkFlow.AttendanceReviewer), true)
                .Warning("Could not create default Action for new Case");
            return;
        }

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(caseDetail.StudentId, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                    .ForContext(nameof(Error), OfferingErrors.NotFound(enrolment.OfferingId), true)
                    .Warning("Could not create default Action for new Case");

                return;
            }

            if (offering.Sessions.Count == 0)
                continue;

            List<string> teacherIds = offering.Teachers
                .Where(entry =>
                    entry.Type == AssignmentType.ClassroomTeacher &&
                    !entry.IsDeleted)
                .Select(entry => entry.StaffId)
                .ToList();

            foreach (string teacherId in teacherIds)
            {
                Staff teacher = await _staffRepository.GetById(teacherId, cancellationToken);

                if (teacher is null)
                    continue;

                Result<CreateSentralEntryAction> action = CreateSentralEntryAction.Create(item.Id, teacher, offering, _currentUserService.UserName);

                if (action.IsFailure)
                {
                    _logger
                        .ForContext(nameof(CaseCreatedDomainEvent), notification, true)
                        .ForContext(nameof(Error), action.Error, true)
                        .Warning("Could not create default Action for new Case");

                    return;
                }

                item.AddAction(action.Value);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
