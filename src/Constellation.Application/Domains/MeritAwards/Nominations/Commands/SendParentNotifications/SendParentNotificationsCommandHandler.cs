namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.SendParentNotifications;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Models.Awards;
using Core.Models.Awards.Enums;
using Core.Models.Awards.Errors;
using Core.Models.Families;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendParentNotificationsCommandHandler
: ICommandHandler<SendParentNotificationsCommand>
{
    private readonly IAwardNominationRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SendParentNotificationsCommandHandler(
        IAwardNominationRepository awardRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<SendParentNotificationsCommand>();
    }

    public async Task<Result> Handle(SendParentNotificationsCommand request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _awardRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(SendParentNotificationsCommand), request, true)
                .ForContext(nameof(Error), AwardNominationPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to send Award Nomination notifications to Parents by user {User}", _currentUserService.UserName);

            return Result.Failure(AwardNominationPeriodErrors.NotFound(request.PeriodId));
        }

        IEnumerable<IGrouping<StudentId, Nomination>> nominationsByStudent = period.Nominations
            .Where(nomination =>
                !nomination.IsDeleted &&
                !period.Notifications.Any(notification =>
                    notification.Nominations.Contains(nomination.Id) &&
                    notification.Type.Equals(AwardNotificationType.Parent)))
            .GroupBy(entry => entry.StudentId);

        foreach (var studentId in nominationsByStudent)
        {
            Student student = await _studentRepository.GetById(studentId.Key, cancellationToken);

            if (student is null)
                continue;

            List<Family> families = await _familyRepository.GetFamiliesByStudentId(studentId.Key, cancellationToken);

            Dictionary<EmailAddress, Name> recipients = new();

            foreach (Family family in families)
            {
                foreach (Parent parent in family.Parents)
                {
                    Result<EmailAddress> emailAddress = EmailAddress.Create(parent.EmailAddress);

                    Result<Name> name = Name.Create(parent.FirstName, string.Empty, parent.LastName);

                    if (emailAddress.IsFailure || name.IsFailure)
                        continue;

                    recipients.TryAdd(emailAddress.Value, name.Value);
                }
            }

            foreach (var recipient in recipients)
            {
                Result<EmailRecipient> emailRecipient = EmailRecipient.Create(recipient.Value, recipient.Key);

                Result<string> emailResult = await _emailService.SendAwardNominationNotificationEmailToParents(
                    [emailRecipient.Value],
                    [],
                    recipient.Value,
                    student.Name,
                    student.CurrentEnrolment?.SchoolName ?? "their partner school",
                    request.DeliveryDate,
                    studentId.ToList(),
                    cancellationToken);

                if (emailResult.IsFailure)
                    continue;

                NominationNotification notification = NominationNotification.Create(
                    request.PeriodId,
                    AwardNotificationType.Parent,
                    studentId.Select(nomination => nomination.Id).ToList(),
                    _dateTime.Now,
                    EmailRecipient.AuroraCollege,
                    [emailRecipient.Value],
                    [],
                    $"Student Awards - {student.Name.DisplayName}",
                    emailResult.Value);

                period.AddNotification(notification);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
