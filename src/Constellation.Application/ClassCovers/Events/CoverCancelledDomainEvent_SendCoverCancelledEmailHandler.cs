namespace Constellation.Application.ClassCovers.Events;

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
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Constellation.Application.DTOs.EmailRequests.ClassworkNotificationTeacherEmail;

internal sealed class CoverCancelledDomainEvent_SendCoverCancelledEmailHandler
    : IDomainEventHandler<CoverCancelledDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger _logger;

    public CoverCancelledDomainEvent_SendCoverCancelledEmailHandler(
        IUnitOfWork unitOfWork,
        IClassCoverRepository classCoverRepository,
        ICourseOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICasualRepository casualRepository,
        ITimetablePeriodRepository periodRepository,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        ITeamRepository teamRepository,
        Serilog.ILogger logger)
    {
        _unitOfWork = unitOfWork;
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
        var cover = await _classCoverRepository.GetById(notification.CoverId, cancellationToken);

        var offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

        var primaryRecipients = new List<EmailAddress>(); // Casual, Classroom Teacher
        var secondaryRecipients = new List<EmailAddress>(); // Head Teacher, Additional Recipients

        var teachers = await _staffRepository.GetCurrentTeachersForOffering(cover.OfferingId, cancellationToken);

        foreach (var teacher in teachers)
        {
            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailAddress.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

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
                var address = EmailAddress.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

                    continue;
                }

                secondaryRecipients.Add(address.Value);
            }
        }

        EmailAddress coveringTeacher = null;

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            var teacher = await _casualRepository.GetById(int.Parse(cover.TeacherId), cancellationToken);

            if (primaryRecipients.All(entry => entry.Email != teacher.EmailAddress) && secondaryRecipients.All(entry => entry.Email != teacher.EmailAddress))
            {
                var address = EmailAddress.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);
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
                var address = EmailAddress.Create(teacher.DisplayName, teacher.EmailAddress);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);
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
            _logger.Error("{action}: Could not create valid email address for covering teacher during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), cover.Id);

            return;
        }

        var additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

        foreach (var teacher in additionalRecipients)
        {
            if (primaryRecipients.All(entry => entry.Email != teacher.Email) && secondaryRecipients.All(entry => entry.Email != teacher.Email))
            {
                var address = EmailAddress.Create(teacher.DisplayName, teacher.Email);

                if (address.IsFailure)
                {
                    _logger.Warning("{action}: Could not create valid email address for {teacher} during processing of cover {id}", nameof(CoverCancelledDomainEvent_SendCoverCancelledEmailHandler), teacher.DisplayName, cover.Id);

                    continue;
                }

                secondaryRecipients.Add(address.Value);
            }
        }

        var teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);

        TimeOnly startTime, endTime;

        if (cover.StartDate == cover.EndDate)
        {
            var periods = await _periodRepository.GetByDayAndOfferingId(cover.StartDate.ToDateTime(TimeOnly.MinValue).GetDayNumber(), cover.OfferingId, cancellationToken);

            startTime = TimeOnly.FromTimeSpan(periods.Min(period => period.StartTime));
            endTime = TimeOnly.FromTimeSpan(periods.Max(period => period.EndTime));
        }

        await _emailService.SendCancelledCoverEmail(cover, offering, coveringTeacher, primaryRecipients, secondaryRecipients, startTime, endTime, teamLink, cancellationToken);
    }
}
