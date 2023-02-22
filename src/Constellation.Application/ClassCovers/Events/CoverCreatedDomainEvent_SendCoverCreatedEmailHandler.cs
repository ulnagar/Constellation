namespace Constellation.Application.ClassCovers.Events;

using Constellation.Application.Abstractions;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CoverCreatedDomainEvent_SendCoverCreatedEmailHandler
    : IDomainEventHandler<CoverCreatedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IOfferingSessionsRepository _sessionRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IEmailAttachmentService _emailAttachmentService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public CoverCreatedDomainEvent_SendCoverCreatedEmailHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        ICourseOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IStudentRepository studentRepository,
        ITeamRepository teamRepository,
        IOfferingSessionsRepository sessionRepository,
        UserManager<AppUser> userManager,
        ITimetablePeriodRepository periodRepository,
        IEmailAttachmentService emailAttachmentService,
        IEmailService emailService,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _classCoverRepository = classCoverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _teamRepository = teamRepository;
        _sessionRepository = sessionRepository;
        _userManager = userManager;
        _periodRepository = periodRepository;
        _emailAttachmentService = emailAttachmentService;
        _emailService = emailService;
        _logger = logger.ForContext<CoverCreatedDomainEvent>();
        _studentRepository = studentRepository;
    }


    public async Task Handle(CoverCreatedDomainEvent notification, CancellationToken cancellationToken)
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
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCreatedDomainEvent_SendCoverCreatedEmailHandler), teacher.DisplayName, cover.Id);

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
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCreatedDomainEvent_SendCoverCreatedEmailHandler), teacher.DisplayName, cover.Id);
                    continue;
                }

                secondaryRecipients.Add(address.Value);
            }
        }

        EmailRecipient coveringTeacher = null;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            var teacher = await _casualRepository.GetById(int.Parse(cover.TeacherId), cancellationToken);

            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailRecipient.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCreatedDomainEvent_SendCoverCreatedEmailHandler), teacher.DisplayName, cover.Id);
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
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCreatedDomainEvent_SendCoverCreatedEmailHandler), teacher.DisplayName, cover.Id);
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
            _logger.Error("{action}: Could not create valid email address for covering teacher during processing of cover {id}", nameof(CoverCreatedDomainEvent_SendCoverCreatedEmailHandler), cover.Id);

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
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCreatedDomainEvent_SendCoverCreatedEmailHandler), teacher.DisplayName, cover.Id);

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

        await _emailService.SendNewCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, startTime, endTime, teamLink, attachments, cancellationToken);
    }
}
