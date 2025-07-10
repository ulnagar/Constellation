namespace Constellation.Application.Domains.Covers.Events.CoverStartAndEndDatesChangedDomainEvent;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Constellation.Core.Models.Covers.Events;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Repositories;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Models.Timetables.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using Extensions;
using Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendCoverUpdatedEmailHandler
    : IDomainEventHandler<CoverStartAndEndDatesChangedDomainEvent>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailAttachmentService _emailAttachmentService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendCoverUpdatedEmailHandler(
        ICoverRepository coverRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IStudentRepository studentRepository,
        ITeamRepository teamRepository,
        IPeriodRepository periodRepository,
        UserManager<AppUser> userManager,
        IEmailAttachmentService emailAttachmentService,
        IEmailService emailService,
        ILogger logger)
    {
        _coverRepository = coverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _studentRepository = studentRepository;
        _teamRepository = teamRepository;
        _periodRepository = periodRepository;
        _userManager = userManager;
        _emailAttachmentService = emailAttachmentService;
        _emailService = emailService;
        _logger = logger.ForContext<CoverStartAndEndDatesChangedDomainEvent>();
    }

    public async Task Handle(CoverStartAndEndDatesChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        Cover cover = await _coverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverStartAndEndDatesChangedDomainEvent), notification.CoverId);

            return;
        }

        if (cover is AccessCover)
            return;

        Offering offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        List<EmailRecipient> primaryRecipients = new(); // Casual, Classroom Teacher
        List<EmailRecipient> secondaryRecipients = new(); // Head Teacher, Additional Recipients

        List<StaffMember> teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (StaffMember teacher in teachers)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress.Email)) 
                continue;

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

            if (address.IsFailure)
            {
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.Name.DisplayName, cover.Id);

                continue;
            }

            primaryRecipients.Add(address.Value);
        }

        List<StaffMember> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (StaffMember teacher in headTeachers)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress.Email) || 
                secondaryRecipients.Any(entry => entry.Email == teacher.EmailAddress.Email)) 
                continue;
            
            Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

            if (address.IsFailure)
            {
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.Name.DisplayName, cover.Id);
                continue;
            }

            secondaryRecipients.Add(address.Value);
        }

        EmailRecipient coveringTeacher = null;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            Casual teacher = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress.Email) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress.Email))
            {
                Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

                if (address.IsFailure)
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.Name.DisplayName, cover.Id);
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (cover.TeacherType == CoverTeacherType.Staff)
        {
            StaffId staffId = StaffId.FromValue(Guid.Parse(cover.TeacherId));

            StaffMember teacher = staffId == StaffId.Empty
                ? null
                : await _staffRepository.GetById(staffId, cancellationToken);

            if (teacher is not null &&
                primaryRecipients.All(entry => entry.Email != teacher.EmailAddress.Email) && 
                secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress.Email))
            {
                Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name, teacher.EmailAddress);

                if (address.IsFailure)
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.Name.DisplayName, cover.Id);
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (coveringTeacher is null)
        {
            _logger.Error("{action}: Could not create valid email address for covering teacher during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), cover.Id);

            return;
        }

        IList<AppUser> additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

        foreach (AppUser teacher in additionalRecipients)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.Email) || 
                secondaryRecipients.Any(entry => entry.Email == teacher.Email)) 
                continue;

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.DisplayName, teacher.Email);

            if (address.IsFailure)
            {
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);

                continue;
            }

            secondaryRecipients.Add(address.Value);
        }

        string teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);

        TimeOnly startTime = TimeOnly.MinValue;
        TimeOnly endTime = TimeOnly.MinValue;
        List<Attachment> attachments = new();

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

            Attachment rollAttachment = await _emailAttachmentService.GenerateClassRollDocument(offering, classStudents, cancellationToken);

            attachments.Add(rollAttachment);
        }

        if (cover.StartDate == cover.EndDate)
        {
            List<Period> periods = await _periodRepository.GetByDayAndOfferingId(cover.StartDate.GetDayNumber(), cover.OfferingId, cancellationToken);

            startTime = TimeOnly.FromTimeSpan(periods.Min(period => period.StartTime));
            endTime = TimeOnly.FromTimeSpan(periods.Max(period => period.EndTime));
        }
        else
        {
            List<Timetable> relevantTimetables = await _offeringRepository.GetTimetableByOfferingId(cover.OfferingId, cancellationToken);
            List<Period> relevantPeriods = await _periodRepository.GetAllFromTimetable(relevantTimetables, cancellationToken);

            Attachment timetableAttachment = await _emailAttachmentService.GenerateClassTimetableDocument(offering, relevantPeriods, cancellationToken);

            attachments.Add(timetableAttachment);
        }

        await _emailService.SendUpdatedCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, notification.PreviousStartDate, startTime, endTime, teamLink, attachments, cancellationToken);
    }
}
