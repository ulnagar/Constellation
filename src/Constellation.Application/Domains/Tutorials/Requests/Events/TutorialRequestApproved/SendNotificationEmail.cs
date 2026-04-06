namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestApproved;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models.AppSettings.Enums;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Events;
using Core.Models.Tutorials.Repositories;
using Core.ValueObjects;
using Extensions;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationEmail
: IDomainEventHandler<TutorialRequestApprovedDomainEvent>
{
    private readonly IAppSettingsService _appSettings;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationEmail(
        IAppSettingsService appSettings,
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _appSettings = appSettings;
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<TutorialRequestApprovedDomainEvent>();
    }

    public async Task Handle(TutorialRequestApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        Request? tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }
        
        List<EmailRecipient> recipients = [];
        
        Student? student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }

        // Who do we send this to?
        TutorialsConfiguration? schedulers = await _appSettings.Tutorials(TutorialPosition.Scheduler, cancellationToken);

        if (schedulers is not null)
        {
            foreach (var scheduler in schedulers.Contacts)
            {
                if (!scheduler.Value.Contains(tutorialRequest.Grade))
                    continue;

                Result<EmailRecipient> schedulerEmail = scheduler.Key.GetEmailRecipient;

                if (schedulerEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                        .ForContext(nameof(Error), schedulerEmail.Error, true)
                        .Warning("Failed to send notification email for approved Tutorial Request");

                    return;
                }

                recipients.Add(schedulerEmail.Value);
            }
        }

        if (recipients.Count == 0)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), ApplicationErrors.ArgumentNull(nameof(recipients)), true)
                .Warning("Failed to send notification email for approved Tutorial Request");

            return;
        }
        
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
