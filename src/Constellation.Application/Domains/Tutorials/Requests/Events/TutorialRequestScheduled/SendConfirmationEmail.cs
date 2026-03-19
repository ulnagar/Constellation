namespace Constellation.Application.Domains.Tutorials.Requests.Events.TutorialRequestScheduled;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Shared;
using Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Events;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Core.ValueObjects;
using Extensions;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendConfirmationEmail
: IDomainEventHandler<TutorialRequestScheduledDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendConfirmationEmail(
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        ITeamRepository teamRepository,
        IStaffRepository staffRepository,
        IPeriodRepository periodRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _teamRepository = teamRepository;
        _staffRepository = staffRepository;
        _periodRepository = periodRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<TutorialRequestScheduledDomainEvent>();
    }

    public async Task Handle(TutorialRequestScheduledDomainEvent notification, CancellationToken cancellationToken)
    {
        Request? tutorialRequest = await _tutorialRepository.GetRequestById(notification.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send confirmation email for scheduled Tutorial");

            return;
        }

        TutorialId tutorialId = tutorialRequest.Plan?.TutorialId ?? TutorialId.Empty;

        if (tutorialId == TutorialId.Empty)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(notification.RequestId), true)
                .Warning("Failed to send confirmation email for scheduled Tutorial");

            return;
        }

        Tutorial? tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(tutorialId), true)
                .Warning("Failed to send confirmation email for scheduled Tutorial");

            return;
        }

        string teamName = tutorial.Teams.FirstOrDefault()?.Name ?? tutorialRequest.Plan.Name;

        List<(string Period, string Teacher)> tutorialSchedule = [];

        List<StaffMember> staff = [];
        List<Period> periods = await _periodRepository.GetAll(cancellationToken);
        List<int> periodDayNumbers = [];

        foreach (TutorialSession session in tutorial.Sessions)
        {
            StaffMember? staffMember = staff.FirstOrDefault(entry => entry.Id == session.StaffId);

            if (staffMember is null)
            {
                staffMember = await _staffRepository.GetById(session.StaffId, cancellationToken);
                staff.Add(staffMember!);
            }

            Period? period = periods.FirstOrDefault(entry => entry.Id == session.PeriodId);

            periodDayNumbers.Add(period.DayNumber);

            tutorialSchedule.Add(new(period.ToString(), staffMember.Name.DisplayName));
        }

        DateOnly firstLessonDate = tutorial.StartDate.GetFirstDayFromCycleAfterDate(periodDayNumbers);
        
        List<EmailRecipient> recipients = [];
        
        Student? student = await _studentRepository.GetById(tutorialRequest.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(tutorialRequest.StudentId), true)
                .Warning("Failed to send confirmation email for scheduled Tutorial");

            return;
        }

        Result<EmailRecipient> studentRecipient = EmailRecipient.Create(student.Name, student.EmailAddress);

        if (studentRecipient.IsSuccess)
            recipients.Add(studentRecipient.Value);
        
        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

        foreach (var family in families)
        {
            StudentFamilyMembership? studentLink = family.Students.FirstOrDefault(link => link.StudentId == student.Id);

            if (studentLink is null || !studentLink.IsResidentialFamily)
                continue;

            foreach (var parent in family.Parents)
            {
                if (recipients.Any(entry => entry.Email == parent.EmailAddress))
                    continue;

                Result<EmailRecipient> parentRecipient = EmailRecipient.Create(parent.Name, parent.EmailAddress);

                if (parentRecipient.IsSuccess)
                    recipients.Add(parentRecipient.Value);
            }

            if (recipients.Any(entry => entry.Email == family.FamilyEmail))
                continue;

            Result<EmailRecipient> familyRecipient = EmailRecipient.Create($"{family.FamilyTitle}", family.FamilyEmail);

            if (familyRecipient.IsSuccess)
                recipients.Add(familyRecipient.Value);
        }

        if (student.CurrentEnrolment is not null && student.CurrentEnrolment.SchoolCode != SchoolCode.Empty)
        {
            List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(student.CurrentEnrolment.SchoolCode, Position.Coordinator, cancellationToken);

            foreach (var contact in contacts)
            {
                if (recipients.Any(entry => entry.Email == contact.EmailAddress.Email))
                    continue;

                Result<EmailRecipient> contactRecipient = contact.GetEmailRecipient();

                if (contactRecipient.IsSuccess)
                    recipients.Add(contactRecipient.Value);
            }
        }

        Result result = await _emailService.SendTutorialRequestScheduledEmail(recipients, tutorialRequest, teamName, tutorialSchedule, firstLessonDate, cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(TutorialRequestScheduledDomainEvent), notification, true)
                .ForContext(nameof(recipients), recipients, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send confirmation email for scheduled Tutorial");
        }
    }
}
