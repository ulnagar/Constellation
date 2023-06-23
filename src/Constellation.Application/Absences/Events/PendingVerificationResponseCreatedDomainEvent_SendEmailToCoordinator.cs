namespace Constellation.Application.Absences.Events;

using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class PendingVerificationResponseCreatedDomainEvent_SendEmailToCoordinator
    : INotificationHandler<PendingVerificationResponseCreatedDomainEvent>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public PendingVerificationResponseCreatedDomainEvent_SendEmailToCoordinator(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _emailService = emailService;
        _logger = logger.ForContext<PendingVerificationResponseCreatedDomainEvent>();
    }

    public Task Handle(PendingVerificationResponseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Send email to ACC to let them know there is a response that requires verification.
    }
}
