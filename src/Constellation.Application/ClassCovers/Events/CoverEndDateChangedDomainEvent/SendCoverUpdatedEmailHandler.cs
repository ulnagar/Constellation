namespace Constellation.Application.ClassCovers.Events.CoverEndDateChangedDomainEvent;

using Abstractions;
using Abstractions.Messaging;
using Extensions;
using Interfaces.Repositories;
using Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions.Repositories;
using Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Core.ValueObjects;
using Core.Models;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.StaffMembers.Repositories;

internal sealed class SendCoverUpdatedEmailHandler
    : IDomainEventHandler<CoverEndDateChangedDomainEvent>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailAttachmentService _emailAttachmentService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendCoverUpdatedEmailHandler(
        IClassCoverRepository classCoverRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IStudentRepository studentRepository,
        ITeamRepository teamRepository,
        ITimetablePeriodRepository periodRepository,
        UserManager<AppUser> userManager,
        IEmailAttachmentService emailAttachmentService,
        IEmailService emailService,
        ILogger logger)
    {
        _classCoverRepository = classCoverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _studentRepository = studentRepository;
        _teamRepository = teamRepository;
        _periodRepository = periodRepository;
        _userManager = userManager;
        _emailAttachmentService = emailAttachmentService;
        _emailService = emailService;
        _logger = logger.ForContext<CoverEndDateChangedDomainEvent>();
    }

    public async Task Handle(CoverEndDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent), notification.CoverId);

            return;
        }

        Offering offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        List<EmailRecipient> primaryRecipients = new(); // Casual, Classroom Teacher
        List<EmailRecipient> secondaryRecipients = new(); // Head Teacher, Additional Recipients

        List<Staff> teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (Staff teacher in teachers)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress)) 
                continue;

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

            if (address.IsFailure)
            {
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);

                continue;
            }

            primaryRecipients.Add(address.Value);
        }

        List<Staff> headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (Staff teacher in headTeachers)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress) || 
                secondaryRecipients.Any(entry => entry.Email == teacher.EmailAddress)) 
                continue;

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

            if (address.IsFailure)
            {
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);
                continue;
            }

            secondaryRecipients.Add(address.Value);
        }

        EmailRecipient coveringTeacher = null;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            Casual teacher = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                Result<EmailRecipient> address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (cover.TeacherType == CoverTeacherType.Staff)
        {
            Staff teacher = await _staffRepository.GetById(cover.TeacherId, cancellationToken);

            if (teacher is not null && 
                primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && 
                secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                Result<EmailRecipient> address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);
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
            if (primaryRecipients.All(entry => entry.Email != teacher.Email) && secondaryRecipients.All(entry => entry.Email != teacher.Email))
            {
                Result<EmailRecipient> address = EmailRecipient.Create(teacher.DisplayName, teacher.Email);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);

                    continue;
                }

                secondaryRecipients.Add(address.Value);
            }
        }

        string teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);
        TimeOnly startTime, endTime;

        List<Attachment> attachments = new();

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOfferingWithSchool(offering.Id, cancellationToken);

            Attachment rollAttachment = await _emailAttachmentService.GenerateClassRollDocument(offering, classStudents, cancellationToken);

            attachments.Add(rollAttachment);
        }

        if (cover.StartDate == cover.EndDate)
        {
            List<TimetablePeriod> periods = await _periodRepository.GetByDayAndOfferingId(cover.StartDate.ToDateTime(TimeOnly.MinValue).GetDayNumber(), cover.OfferingId, cancellationToken);

            startTime = TimeOnly.FromTimeSpan(periods.Min(period => period.StartTime));
            endTime = TimeOnly.FromTimeSpan(periods.Max(period => period.EndTime));
        }
        else
        {
            List<string> relevantTimetables = await _offeringRepository.GetTimetableByOfferingId(cover.OfferingId, cancellationToken);
            List<TimetablePeriod> relevantPeriods = await _periodRepository.GetAllFromTimetable(relevantTimetables, cancellationToken);

            Attachment timetableAttachment = await _emailAttachmentService.GenerateClassTimetableDocument(offering, relevantPeriods, cancellationToken);

            attachments.Add(timetableAttachment);
        }

        await _emailService.SendUpdatedCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, cover.StartDate, startTime, endTime, teamLink, attachments, cancellationToken);
    }
}
