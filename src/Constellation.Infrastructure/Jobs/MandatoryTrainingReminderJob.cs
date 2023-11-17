namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MandatoryTrainingReminderJob : IMandatoryTrainingReminderJob
{
    private readonly AppConfiguration _configuration;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public MandatoryTrainingReminderJob(
        IOptions<AppConfiguration> configuration,
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository schoolContactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _configuration = configuration.Value;
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _schoolRepository = schoolRepository;
        _schoolContactRepository = schoolContactRepository;
        _emailService = emailService;
        _logger = logger.ForContext<IMandatoryTrainingReminderJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        List<CompletionRecordExtendedDetailsDto> detailedRecords = new();

        // - Get all modules
        List<TrainingModule> modules = await _trainingModuleRepository.GetAllCurrent(cancellationToken);

        // - Get all staff
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        foreach (TrainingModule module in modules)
        {
            foreach (Staff staffMember in staff)
            {
                List<TrainingCompletion> records = module.Completions
                    .Where(record =>
                        record.StaffId == staffMember.StaffId &&
                        !record.IsDeleted)
                    .ToList();

                TrainingCompletion record = records
                    .OrderByDescending(record =>
                        (record.CompletedDate.HasValue) ? record.CompletedDate.Value : record.CreatedAt)
                    .FirstOrDefault();

                CompletionRecordExtendedDetailsDto entry = new();
                entry.AddModuleDetails(module);
                entry.AddStaffDetails(staffMember);

                List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staffMember.StaffId, cancellationToken);

                foreach (Faculty faculty in faculties)
                {
                    List<Staff> headTeachers = faculty
                        .Members
                        .Where(member =>
                            !member.IsDeleted &&
                            member.Role == Core.Enums.FacultyMembershipRole.Manager)
                        .Select(member => member.Staff)
                        .ToList();

                    foreach (Staff headTeacher in headTeachers)
                    {
                        entry.AddHeadTeacherDetails(faculty, headTeacher);
                    }
                }

                if (staffMember.IsShared)
                {
                    School sharedSchool = await _schoolRepository.GetById(staffMember.SchoolCode, cancellationToken);

                    List<SchoolContact> sharedSchoolPrincipals = await _schoolContactRepository.GetPrincipalsForSchool(staffMember.SchoolCode, cancellationToken);

                    foreach (SchoolContact principal in sharedSchoolPrincipals)
                    {
                        entry.AddPrincipalDetails(principal, sharedSchool);
                    }
                }

                School localSchool = await _schoolRepository.GetById("8912", cancellationToken);

                List<SchoolContact> localPrincipals = await _schoolContactRepository.GetPrincipalsForSchool(localSchool.Code, cancellationToken);

                foreach (SchoolContact principal in localPrincipals)
                {
                    entry.AddPrincipalDetails(principal, localSchool);
                }

                if (record is not null)
                    entry.AddRecordDetails(record);

                entry.CalculateExpiry();

                detailedRecords.Add(entry);
            }
        }

        // - Remove all entries that are not due for expiry within 30 days, or have not already expired
        List<CompletionRecordExtendedDetailsDto> latestRecords = detailedRecords
            .Where(record => record.TimeToExpiry <= 30)
            .ToList();

        // Group by staff member
        // - Send emails to each staff member
        foreach (var teacher in latestRecords.GroupBy(record => record.StaffEntry))
        {
            await SendEmail(teacher.ToList(), cancellationToken);
        }
    }

    public async Task SendEmail(List<CompletionRecordExtendedDetailsDto> entries, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> warning = entries
            .Where(record => 
                record.TimeToExpiry <= 30 && 
                record.TimeToExpiry > 14)
            .ToDictionary(
                record => $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", 
                record => (record.DueDate.HasValue) ? record.DueDate.Value.ToShortDateString() : "Not Yet Completed");

        Dictionary<string, string> alert = entries
            .Where(record => 
                record.TimeToExpiry <= 14 && 
                record.TimeToExpiry > 0)
            .ToDictionary(
                record => $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", 
                record => (record.DueDate.HasValue) ? record.DueDate.Value.ToShortDateString() : "Not Yet Completed");

        Dictionary<string, string> expired = entries
            .Where(record => record.TimeToExpiry <= 0)
            .ToDictionary(
                record => $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})",
                record => (record.DueDate.HasValue) ? record.DueDate.Value.ToShortDateString() : "Not Yet Completed");

        List<EmailRecipient> warningRecipients = new();

        warningRecipients.Add(entries.First().StaffEntry);

        List<EmailRecipient> alertRecipients = new(warningRecipients);

        IEnumerable<EmailRecipient> headTeachers = entries
            .SelectMany(entry => entry.StaffHeadTeachers)
            .Distinct();

        foreach (EmailRecipient teacher in headTeachers)
        {
            if (alertRecipients.All(entry => entry != teacher))
                alertRecipients.Add(teacher);
        }

        List<EmailRecipient> expiredRecipients = new(alertRecipients);

        foreach (string coordinatorId in _configuration.MandatoryTraining.CoordinatorIds)
        {
            Staff member = await _staffRepository.GetById(coordinatorId, cancellationToken);
            if (member is not null)
            {
                Result<EmailRecipient> coordinatorRequest = EmailRecipient.Create(member.DisplayName, member.EmailAddress);

                if (coordinatorRequest.IsFailure)
                {
                    _logger
                    .ForContext("Error", coordinatorRequest.Error)
                        .Warning("Could not generate EmailRecipient for {staff}", member.DisplayName);

                    continue;
                }

                if (expiredRecipients.All(entry => entry != coordinatorRequest.Value))
                    expiredRecipients.Add(coordinatorRequest.Value);
            }
        }

        IEnumerable<EmailRecipient> principals = entries
            .SelectMany(entry => entry.PrincipalContacts)
            .Distinct();

        foreach (EmailRecipient principal in entries.First().PrincipalContacts)
        {
            if (expiredRecipients.All(entry => entry != principal))
                expiredRecipients.Add(principal);
        }

        if (warning.Count > 0)
            await _emailService.SendTrainingExpiryWarningEmail(warning, warningRecipients);

        if (alert.Count > 0)
            await _emailService.SendTrainingExpiryAlertEmail(alert, alertRecipients);

        if (expired.Count > 0)
            await _emailService.SendTrainingExpiredEmail(expired, expiredRecipients);
    }
}