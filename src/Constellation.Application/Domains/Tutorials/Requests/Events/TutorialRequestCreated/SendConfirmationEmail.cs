namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestCreated;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Shared;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Events;
using Core.Models.Tutorials.Repositories;
using Core.ValueObjects;
using Interfaces.Gateways;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendConfirmationEmail
: IDomainEventHandler<TutorialRequestCreatedDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendConfirmationEmail(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(TutorialRequestCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send confirmation email for Tutorial Request");

            return;
        }
        
        List<EmailRecipient> recipients = [];
        
        Student student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to send confirmation email for Tutorial Request");

            return;
        }

        Result<EmailRecipient> studentRecipient = EmailRecipient.Create(student.Name, student.EmailAddress);

        if (studentRecipient.IsSuccess)
            recipients.Add(studentRecipient.Value);
        
        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

        // Should this only go to Residential parents?
        foreach (var family in families)
        {
            
        }

        List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(student.CurrentEnrolment?.SchoolCode, Position.Coordinator, cancellationToken);




        await _emailService.SendTutorialRequestReceivedEmail()
    }
}
