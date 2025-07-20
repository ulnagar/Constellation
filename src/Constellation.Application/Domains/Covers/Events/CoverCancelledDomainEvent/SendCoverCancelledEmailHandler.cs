namespace Constellation.Application.Domains.Covers.Events.CoverCancelledDomainEvent;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Constellation.Core.Models.Covers.Events;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Repositories;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Covers.Enums;
using Core.Models.Covers.Errors;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
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

internal sealed class SendCoverCancelledEmailHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger _logger;

    public SendCoverCancelledEmailHandler(
        ICoverRepository coverRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IPeriodRepository periodRepository,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        ITeamRepository teamRepository,
        ILogger logger)
    {
        _coverRepository = coverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _periodRepository = periodRepository;
        _userManager = userManager;
        _emailService = emailService;
        _teamRepository = teamRepository;
        _logger = logger
            .ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        Cover cover = await _coverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger
                .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                .ForContext(nameof(Error), CoverErrors.NotFound(notification.CoverId), true)
                .Warning("Failed to send Cover Cancelled Email notification");

            return;
        }

        if (cover is AccessCover)
            return;

        Offering offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                .ForContext(nameof(Cover), cover, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(cover.OfferingId))
                .Warning("Failed to send Cover Cancelled Email notification");

            return;
        }

        List<EmailRecipient> primaryRecipients = []; // Casual, Classroom Teacher
        List<EmailRecipient> secondaryRecipients = []; // Head Teacher, Additional Recipients

        List<StaffMember> teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (StaffMember teacher in teachers)
        {
            if (primaryRecipients.Any(entry => entry.Email == teacher.EmailAddress.Email))
                continue;

            Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name.DisplayName, teacher.EmailAddress.Email);

            if (address.IsFailure)
            {
                _logger
                    .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                    .ForContext(nameof(Error), address.Error, true)
                    .Warning("Failed to send Cover Cancelled Email notification");

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
                _logger
                    .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                    .ForContext(nameof(Error), address.Error, true)
                    .Warning("Failed to send Cover Cancelled Email notification");

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
                    _logger
                        .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                        .ForContext(nameof(Error), address.Error, true)
                        .ForContext(nameof(EmailAddress), teacher.EmailAddress.Email)
                        .Warning("Failed to send Cover Cancelled Email notification");
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
                Result<EmailRecipient> address = EmailRecipient.Create(teacher.Name.DisplayName, teacher.EmailAddress.Email);

                if (address.IsFailure)
                    _logger
                        .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                        .ForContext(nameof(Error), address.Error, true)
                        .ForContext(nameof(EmailAddress), teacher.EmailAddress.Email)
                        .Warning("Failed to send Cover Cancelled Email notification");
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (coveringTeacher is null)
        {
            _logger
                .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(StaffId.Empty), true)
                .Warning("Failed to send Cover Cancelled Email notification");

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
                _logger
                    .ForContext(nameof(CoverCancelledDomainEvent), notification, true)
                    .ForContext(nameof(Error), address.Error, true)
                    .ForContext(nameof(EmailAddress), teacher.Email)
                    .Warning("Failed to send Cover Cancelled Email notification");

                continue;
            }

            secondaryRecipients.Add(address.Value);
        }

        string teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);

        TimeOnly startTime = TimeOnly.MinValue;
        TimeOnly endTime = TimeOnly.MinValue;

        if (cover.StartDate == cover.EndDate)
        {
            List<Period> periods = await _periodRepository.GetByDayAndOfferingId(cover.StartDate.GetDayNumber(), cover.OfferingId, cancellationToken);

            startTime = TimeOnly.FromTimeSpan(periods.Min(period => period.StartTime));
            endTime = TimeOnly.FromTimeSpan(periods.Max(period => period.EndTime));
        }

        await _emailService.SendCancelledCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, startTime, endTime, teamLink, new List<Attachment>(), cancellationToken);
    }
}
