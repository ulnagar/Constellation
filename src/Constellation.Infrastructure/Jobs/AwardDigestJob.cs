namespace Constellation.Infrastructure.Jobs;

using Application.Domains.MeritAwards.Awards.Models;
using Application.Interfaces.Jobs;
using Application.Interfaces.Services;
using Application.Models.Identity.Repositories;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.Auth.Enums;
using Core.Models.Awards;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;

internal sealed class AwardDigestJob : IAwardDigestJob
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AwardDigestJob(
        IStudentAwardRepository awardRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        IIdentityRepository identityRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _identityRepository = identityRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<IAwardDigestJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        DateTime cutoff = DateTime.Today.AddDays(-7);

        List<StudentAward> recentAwards = await _awardRepository.GetAllIssuedAfter(cutoff, cancellationToken);

        List<StudentId> studentIds = recentAwards
            .Select(entry => entry.StudentId)
            .Distinct()
            .ToList();

        List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

        List<IGrouping<SchoolCode, Student>> schools =
            students.GroupBy(entry => entry.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty)
                .OrderBy(entry => entry.Key.Value).ToList();

        foreach (IGrouping<SchoolCode, Student> schoolGroup in schools)
        {
            _logger
                .ForContext(nameof(SchoolCode), schoolGroup.Key.Value)
                .Information("Processing {school}", schoolGroup.First().CurrentEnrolment?.SchoolName ?? "Unknown School");

            List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(schoolGroup.Key, Position.Coordinator, cancellationToken);

            if (contacts.Count == 0)
                continue;

            List<EmailRecipient> recipients = [];

            foreach (SchoolContact contact in contacts)
            {
                bool optedIn = await _identityRepository.UserHasOptedInToNotification(
                    contact.EmailAddress.Value, 
                    NotificationType.AwardsDigest, 
                    cancellationToken);

                if (!optedIn)
                    continue;

                Result<EmailRecipient> recipient = contact.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }

            if (recipients.Count == 0)
            {
                _logger
                    .ForContext(nameof(SchoolCode), schoolGroup.Key.Value)
                    .Information("Found no valid School Contact recipients. Skipping...");

                continue;
            }

            List<StudentAwardTally> schoolStudentAwards = [];

            foreach (Student student in schoolGroup)
            {
                List<StudentAward> studentAwards = recentAwards.Where(entry => entry.StudentId == student.Id).ToList();
                int astras = studentAwards.Count(entry => entry.Type == "Astra Award");
                int stellars = studentAwards.Count(entry => entry.Type == "Stellar Award");
                int galaxies = studentAwards.Count(entry => entry.Type == "Galaxy Medal");
                int universals = studentAwards.Count(entry => entry.Type == "Aurora Universal Achiever");
                int other = studentAwards.Count - (astras + stellars + galaxies + universals);

                schoolStudentAwards.Add(new(
                    student.Name,
                    student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                    astras,
                    stellars, 
                    galaxies, 
                    universals, 
                    other));
            }

            _logger
                .ForContext(nameof(SchoolCode), schoolGroup.Key.Value)
                .Information("Sending email");

            Result success = await _emailService.SendAwardDigestToSchools(recipients, schoolStudentAwards, cancellationToken);

            if (success.IsFailure)
            {
                _logger
                    .ForContext(nameof(SchoolCode), schoolGroup.Key.Value)
                    .Warning("Failed to send Awards Digest for {school}", schoolGroup.First().CurrentEnrolment?.SchoolName ?? "Unknown School");
            }
        }
    }
}