namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestCreated;

using Abstractions.Messaging;
using AppSettings.Models;
using Constellation.Application.Extensions;
using Constellation.Core.Errors;
using Constellation.Core.Models.AppSettings.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Shared;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Events;
using Core.Models.Tutorials.Repositories;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendNotificationEmail
: IDomainEventHandler<TutorialRequestCreatedDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAppSettingsService _appSettings;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationEmail(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IAppSettingsService appSettings,
        IEmailService emailService,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _appSettings = appSettings;
        _emailService = emailService;
        _logger = logger
            .ForContext<TutorialRequestCreatedDomainEvent>();
    }

    public async Task Handle(TutorialRequestCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Request? tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send notification email for Tutorial Request");

            return;
        }
        
        List<EmailRecipient> recipients = [];
        
        Student? student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to send notification email for Tutorial Request");

            return;
        }

        // Who do we send this to?
        TutorialsConfiguration? approvers = await _appSettings.Tutorials(TutorialPosition.Approver, cancellationToken);

        if (approvers is not null)
        {
            foreach (var approver in approvers.Contacts)
            {
                if (!approver.Value.Contains(tutorialRequest.Grade))
                    continue;

                Result<EmailRecipient> approverEmail = approver.Key.GetEmailRecipient();

                if (approverEmail.IsFailure)
                {
                    _logger
                        .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                        .ForContext(nameof(Error), approverEmail.Error, true)
                        .Warning("Failed to send notification email for Tutorial Request");

                    return;
                }

                recipients.Add(approverEmail.Value);
            }
        }

        if (recipients.Count == 0)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), ApplicationErrors.ArgumentNull(nameof(recipients)), true)
                .Warning("Failed to send notification email for Tutorial Request");

            return;
        }

        Result result = await _emailService.SendTutorialRequestReceivedNotificationEmail(recipients, tutorialRequest, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(TutorialRequestCreatedDomainEvent), notification, true)
                .ForContext(nameof(recipients), recipients, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send notification email for Tutorial Request");
        }
    }
}
