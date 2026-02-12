namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestApproved;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Shared;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Events;
using Core.Models.Tutorials.Repositories;
using Core.ValueObjects;
using Interfaces.Configuration;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationEmail
: IDomainEventHandler<TutorialRequestApprovedDomainEvent>
{
    private readonly AppConfiguration _configuration;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationEmail(
        IOptions<AppConfiguration> configuration,
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _configuration = configuration.Value;
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<TutorialRequestApprovedDomainEvent>();
    }

    public async Task Handle(TutorialRequestApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }
        
        List<EmailRecipient> recipients = [];
        
        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }

        // Who do we send this to?
        EmployeeId schedulerEmpId = _configuration.Tutorials.Scheduler ?? EmployeeId.Empty;

        if (schedulerEmpId == EmployeeId.Empty)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.InvalidId, true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }

        StaffMember staffMember = await _staffRepository.GetByEmployeeId(schedulerEmpId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(schedulerEmpId), true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }

        Result<EmailRecipient> recipient = EmailRecipient.Create(staffMember.Name, staffMember.EmailAddress);

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }

        recipients.Add(recipient.Value);

        Result result = await _emailService.SendTutorialRequestApprovedNotificationEmail(recipients, tutorialRequest, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(recipients), recipients, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send notification email for approved Tutorial Request");
        }
    }
}
