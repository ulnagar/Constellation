namespace Constellation.Application.Domains.Covers.Events.CoverEndDateChangedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Domains.Covers.Events.CoverCreatedDomainEvent;
using Constellation.Core.Models.Covers.Events;
using Constellation.Core.Models.Covers.Repositories;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Core.Abstractions.Repositories;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
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
using Interfaces.Configuration;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendCoverUpdatedEmailHandler
    : IDomainEventHandler<CoverEndDateChangedDomainEvent>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IEmailAttachmentService _emailAttachmentService;
    private readonly IEmailService _emailService;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;

    public SendCoverUpdatedEmailHandler(
        ICoverRepository coverRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IStudentRepository studentRepository,
        ITeamRepository teamRepository,
        IPeriodRepository periodRepository,
        IEmailAttachmentService emailAttachmentService,
        IEmailService emailService,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _coverRepository = coverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _studentRepository = studentRepository;
        _teamRepository = teamRepository;
        _periodRepository = periodRepository;
        _emailAttachmentService = emailAttachmentService;
        _emailService = emailService;
        _configuration = configuration.Value;
        _logger = logger.ForContext<CoverEndDateChangedDomainEvent>();
    }

    public async Task Handle(CoverEndDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        Cover? cover = await _coverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverEndDateChangedDomainEvent), notification.CoverId);

            return;
        }

        if (cover is AccessCover)
            return;

        Offering? offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(SendCoverCreatedEmailHandler), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(cover.OfferingId), true)
                .Warning("Failed to send Cover Updated Email notification");
            
            return;
        }

        List<EmailRecipient> primaryRecipients = new(); // Casual, Classroom Teacher
        List<EmailRecipient> secondaryRecipients = new(); // Head Teacher, Additional Recipients

        List<StaffMember> teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (StaffMember teacher in teachers)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress.Email)) 
                continue;

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name.DisplayName, teacher.EmailAddress.Email);

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

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name.DisplayName, teacher.EmailAddress.Email);

            if (address.IsFailure)
            {
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverUpdatedEmailHandler), teacher.Name.DisplayName, cover.Id);
                continue;
            }

            secondaryRecipients.Add(address.Value);
        }

        EmailRecipient? coveringTeacher = null;

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

            StaffMember? teacher = staffId == StaffId.Empty
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

        foreach (EmployeeId employeeId in _configuration.Covers.CoverContacts)
        {
            StaffMember? teacher = await _staffRepository.GetByEmployeeId(employeeId, cancellationToken);

            if (teacher is null)
            {
                _logger
                    .ForContext(nameof(SendCoverCreatedEmailHandler), notification, true)
                    .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(employeeId), true)
                    .ForContext(nameof(EmployeeId), employeeId)
                    .Warning("Failed to send Cover Updated Email notification");

                continue;
            }

            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress) ||
                secondaryRecipients.Any(entry => entry.Email == teacher.EmailAddress))
                continue;

            Result<EmailRecipient> address = teacher.GetEmailRecipient();

            if (address.IsFailure)
            {
                _logger
                    .ForContext(nameof(SendCoverCreatedEmailHandler), notification, true)
                    .ForContext(nameof(Error), address.Error, true)
                    .ForContext(nameof(EmailAddress), teacher.EmailAddress)
                    .Warning("Failed to send Cover Updated Email notification");

                continue;
            }

            secondaryRecipients.Add(address.Value);
        }

        string? teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(CultureInfo.InvariantCulture), cancellationToken);

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

        await _emailService.SendUpdatedCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, cover.StartDate, startTime, endTime, teamLink ?? string.Empty, attachments, cancellationToken);
    }
}
