namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestApproved;

using Abstractions.Messaging;
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
: IDomainEventHandler<TutorialRequestApprovedDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendNotificationEmail(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(TutorialRequestApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send confirmation email for Tutorial Request");

            return;
        }
        
        List<EmailRecipient> recipients = [];
        
        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to send confirmation email for Tutorial Request");

            return;
        }

        // Who do we send this to?

        Result result = await _emailService.SendTutorialRequestApprovedNotificationEmail(recipients, tutorialRequest, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(TutorialRequestApprovedDomainEvent), notification, true)
                .ForContext(nameof(recipients), recipients, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send confirmation email for Tutorial Request");
        }
    }
}
