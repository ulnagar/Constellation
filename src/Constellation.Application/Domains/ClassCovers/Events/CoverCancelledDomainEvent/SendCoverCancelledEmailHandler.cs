namespace Constellation.Application.Domains.ClassCovers.Events.CoverCancelledDomainEvent;

using Abstractions.Messaging;
using Application.Models.Auth;
using Application.Models.Identity;
using Core.Abstractions.Repositories;
using Core.DomainEvents;
using Core.Models;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
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
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger _logger;

    public SendCoverCancelledEmailHandler(
        IClassCoverRepository classCoverRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        IPeriodRepository periodRepository,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        ITeamRepository teamRepository,
        ILogger logger)
    {
        _classCoverRepository = classCoverRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _casualRepository = casualRepository;
        _periodRepository = periodRepository;
        _userManager = userManager;
        _emailService = emailService;
        _teamRepository = teamRepository;
        _logger = logger.ForContext<CoverCancelledDomainEvent>();
    }

    public async Task Handle(CoverCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        // Gather details
        ClassCover cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        if (cover is null)
        {
            _logger.Error("{action}: Could not find cover with Id {id} in database", nameof(CoverCancelledDomainEvent), notification.CoverId);

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
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

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
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

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
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);
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
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);
                else
                {
                    primaryRecipients.Add(address.Value);
                    coveringTeacher = address.Value;
                }
            }
        }

        if (coveringTeacher is null)
        {
            _logger.Error("{action}: Could not create valid email address for covering teacher during processing of cover {id}", nameof(SendCoverCancelledEmailHandler), cover.Id);

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
                _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

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
