namespace Constellation.Application.ClassCovers.Events;

using Constellation.Application.Abstractions;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Net.Mail;
using System;
using System.Threading;
using System.Threading.Tasks;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Serilog;
using System.Linq;
using Constellation.Application.Extensions;

internal sealed class CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler
    : IDomainEventHandler<CoverStartAndEndDatesChangedDomainEvent>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IOfferingSessionsRepository _sessionRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailAttachmentService _emailAttachmentService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler(
        IClassCoverRepository classCoverRepository,
        ICourseOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IStudentRepository studentRepository,
        ITeamRepository teamRepository,
        ITimetablePeriodRepository periodRepository,
        IOfferingSessionsRepository sessionRepository,
        UserManager<AppUser> userManager,
        IEmailAttachmentService emailAttachmentService,
        IEmailService emailService,
        Serilog.ILogger logger)
    {
        _classCoverRepository = classCoverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _studentRepository = studentRepository;
        _teamRepository = teamRepository;
        _periodRepository = periodRepository;
        _sessionRepository = sessionRepository;
        _userManager = userManager;
        _emailAttachmentService = emailAttachmentService;
        _emailService = emailService;
        _logger = logger.ForContext<CoverStartAndEndDatesChangedDomainEvent>();
    }

    public async Task Handle(CoverStartAndEndDatesChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        var offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        var primaryRecipients = new List<EmailRecipient>(); // Casual, Classroom Teacher
        var secondaryRecipients = new List<EmailRecipient>(); // Head Teacher, Additional Recipients

        var teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (var teacher in teachers)
        {
            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);

                    continue;
                }

                primaryRecipients.Add(address.Value);
            }
        }

        var headTeachers = await _staffRepository.GetFacultyHeadTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (var teacher in headTeachers)
        {
            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);
                    continue;
                }

                secondaryRecipients.Add(address.Value);
            }
        }

        EmailRecipient coveringTeacher = null;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            var teacher = await _casualRepository.GetById(Guid.Parse(cover.TeacherId), cancellationToken);

            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);
                }
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (cover.TeacherType == CoverTeacherType.Staff)
        {
            var teacher = await _staffRepository.GetById(cover.TeacherId, cancellationToken);

            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);
                }
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (coveringTeacher is null)
        {
            _logger.Error("{action}: Could not create valid email address for covering teacher during processing of cover {id}", nameof(CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler), cover.Id);

            return;
        }

        var additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

        foreach (var teacher in additionalRecipients)
        {
            if (primaryRecipients.All(entry => entry.Email != teacher.Email) && secondaryRecipients.All(entry => entry.Email != teacher.Email))
            {
                var address = EmailRecipient.Create(teacher.DisplayName, teacher.Email);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverStartAndEndDatesChangedDomainEvent_SendCoverUpdatedEmailHandler), teacher.DisplayName, cover.Id);

                    continue;
                }

                secondaryRecipients.Add(address.Value);
            }
        }

        var teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);
        TimeOnly startTime, endTime;
        var attachments = new List<Attachment>();

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            var classStudents = await _studentRepository.GetCurrentEnrolmentsForOfferingWithSchool(offering.Id, cancellationToken);

            var rollAttachment = await _emailAttachmentService.GenerateClassRollDocument(offering, classStudents, cancellationToken);

            attachments.Add(rollAttachment);
        }

        if (cover.StartDate == cover.EndDate)
        {
            var periods = await _periodRepository.GetByDayAndOfferingId(cover.StartDate.ToDateTime(TimeOnly.MinValue).GetDayNumber(), cover.OfferingId, cancellationToken);

            startTime = TimeOnly.FromTimeSpan(periods.Min(period => period.StartTime));
            endTime = TimeOnly.FromTimeSpan(periods.Max(period => period.EndTime));
        }
        else
        {
            var sessions = await _sessionRepository.GetByOfferingId(cover.OfferingId, cancellationToken);
            var relevantTimetables = await _sessionRepository.GetTimetableByOfferingId(cover.OfferingId, cancellationToken);
            var relevantPeriods = await _periodRepository.GetAllFromTimetable(relevantTimetables, cancellationToken);

            var timetableAttachment = await _emailAttachmentService.GenerateClassTimetableDocument(offering, sessions, relevantPeriods, cancellationToken);

            attachments.Add(timetableAttachment);
        }

        await _emailService.SendUpdatedCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, notification.PreviousStartDate, startTime, endTime, teamLink, attachments, cancellationToken);
    }
}
