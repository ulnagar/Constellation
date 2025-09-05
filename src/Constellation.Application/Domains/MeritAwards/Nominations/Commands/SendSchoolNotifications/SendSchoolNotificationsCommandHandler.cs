namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.SendSchoolNotifications;

using Abstractions.Messaging;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Awards.Enums;
using Constellation.Core.Models.Awards.Errors;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
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

internal sealed class SendSchoolNotificationsCommandHandler
: ICommandHandler<SendSchoolNotificationsCommand>
{
    private readonly IAwardNominationRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SendSchoolNotificationsCommandHandler(
        IAwardNominationRepository awardRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<SendSchoolNotificationsCommand>();
    }

    public async Task<Result> Handle(SendSchoolNotificationsCommand request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _awardRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(SendSchoolNotificationsCommand), request, true)
                .ForContext(nameof(Error), AwardNominationPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to send Award Nomination notifications to Parents by user {User}", _currentUserService.UserName);

            return Result.Failure(AwardNominationPeriodErrors.NotFound(request.PeriodId));
        }

        List<StudentId> studentIds = period.Nominations
            .Where(nomination => 
                !nomination.IsDeleted &&
                !period.Notifications.Any(notification =>
                    notification.Nominations.Contains(nomination.Id) &&
                    notification.Type.Equals(AwardNotificationType.PartnerSchool)))
            .Select(entry => entry.StudentId)
            .ToList();

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        IEnumerable<IGrouping<string, Student>> studentsBySchool = students.GroupBy(student => student.CurrentEnrolment?.SchoolCode ?? string.Empty);

        foreach (var schoolGroup in studentsBySchool)
        {
            string school = schoolGroup.First().CurrentEnrolment?.SchoolName;

            Dictionary<Name, List<Nomination>> schoolStudents = new();

            foreach (var schoolStudent in schoolGroup)
            {
                schoolStudents.TryAdd(
                    schoolStudent.Name,
                    period.Nominations
                        .Where(nomination => 
                            !nomination.IsDeleted && 
                            nomination.StudentId == schoolStudent.Id)
                        .ToList());
            }

            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(schoolGroup.Key, cancellationToken);

            List<EmailRecipient> toRecipients = [];
            List<EmailRecipient> ccRecipients = [];
            Name principalName = null;

            foreach (SchoolContact contact in contacts)
            {
                if (contact.Assignments.Any(role =>
                        !role.IsDeleted &&
                        role.Role.Equals(Position.Principal) &&
                        role.SchoolCode == schoolGroup.Key))
                {
                    Result<EmailAddress> emailAddress = EmailAddress.Create(contact.EmailAddress);

                    Result<Name> name = Name.Create(contact.FirstName, string.Empty, contact.LastName);

                    if (emailAddress.IsFailure || name.IsFailure)
                        continue;

                    principalName ??= name.Value;

                    Result<EmailRecipient> recipient = EmailRecipient.Create(name.Value, emailAddress.Value);

                    if (recipient.IsFailure)
                        continue;

                    toRecipients.Add(recipient.Value);
                }

                if (contact.Assignments.Any(role =>
                        !role.IsDeleted &&
                        role.Role.Equals(Position.Coordinator) &&
                        role.SchoolCode == schoolGroup.Key))
                {
                    Result<EmailAddress> emailAddress = EmailAddress.Create(contact.EmailAddress);

                    Result<Name> name = Name.Create(contact.FirstName, string.Empty, contact.LastName);

                    if (emailAddress.IsFailure || name.IsFailure)
                        continue;

                    Result<EmailRecipient> recipient = EmailRecipient.Create(name.Value, emailAddress.Value);

                    if (recipient.IsFailure)
                        continue;

                    ccRecipients.Add(recipient.Value);
                }
            }

            Result<string> emailResult = await _emailService.SendAwardNominationNotificationEmailToSchools(
                toRecipients,
                ccRecipients,
                principalName,
                school,
                request.DeliveryDate,
                schoolStudents,
                cancellationToken);

            if (emailResult.IsFailure)
                continue;

            NominationNotification notification = NominationNotification.Create(
                request.PeriodId,
                AwardNotificationType.PartnerSchool,
                schoolStudents.SelectMany(entry => entry.Value).Select(nomination => nomination.Id).ToList(),
                _dateTime.Now,
                EmailRecipient.AuroraCollege,
                toRecipients,
                ccRecipients,
                "Student Awards",
                emailResult.Value);

            period.AddNotification(notification);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
