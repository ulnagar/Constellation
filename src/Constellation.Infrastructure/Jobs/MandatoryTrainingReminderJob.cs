namespace Constellation.Infrastructure.Jobs;

using Application.Training.Models;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.Faculty.ValueObjects;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
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
    private readonly ITrainingRoleRepository _trainingRoleRepository;
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public MandatoryTrainingReminderJob(
        IOptions<AppConfiguration> configuration,
        ITrainingRoleRepository trainingRoleRepository,
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository schoolContactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _configuration = configuration.Value;
        _trainingRoleRepository = trainingRoleRepository;
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

        // Get all staff
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        foreach (Staff staffMember in staff)
        {
            // Get all roles for the staff member
            List<TrainingRole> roles = await _trainingRoleRepository.GetRolesForStaffMember(staffMember.StaffId, cancellationToken);

            // Get all modules attached to roles
            List<TrainingModuleId> moduleIds = roles
                .SelectMany(role => role.Modules)
                .Select(module => module.ModuleId)
                .ToList();
            
            // Get all completions for staff member
            foreach (TrainingModuleId moduleId in moduleIds)
            {
                TrainingModule module = await _trainingModuleRepository.GetModuleById(moduleId, cancellationToken);

                if (module.IsDeleted) continue;

                TrainingCompletion completion = module
                    .Completions
                    .Where(record =>
                        record.StaffId == staffMember.StaffId &&
                        !record.IsDeleted)
                    .MaxBy(record => record.CompletedDate);

                CompletionRecordExtendedDetailsDto entry = new();
                entry.AddModuleDetails(module);
                entry.AddStaffDetails(staffMember);

                List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staffMember.StaffId, cancellationToken);

                foreach (Faculty faculty in faculties)
                {
                    List<string> headTeacherIds = faculty
                        .Members
                        .Where(member =>
                            !member.IsDeleted &&
                            member.Role == FacultyMembershipRole.Manager)
                        .Select(member => member.StaffId)
                        .ToList();

                    List<Staff> headTeachers = await _staffRepository.GetListFromIds(headTeacherIds, cancellationToken);

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

                if (completion is not null)
                    entry.AddRecordDetails(completion);

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
        foreach (IGrouping<EmailRecipient, CompletionRecordExtendedDetailsDto> teacher in latestRecords.GroupBy(record => record.StaffEntry))
        {
            await SendEmail(teacher.ToList(), cancellationToken);
        }
    }

    public async Task SendEmail(List<CompletionRecordExtendedDetailsDto> entries, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> warning = entries
            .Where(record => 
                record.TimeToExpiry is <= 30 and > 14)
            .ToDictionary(
                record => $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", 
                record => (record.DueDate.HasValue) ? record.DueDate.Value.ToShortDateString() : "Not Yet Completed");

        Dictionary<string, string> alert = entries
            .Where(record => 
                record.TimeToExpiry is <= 14 and > 0)
            .ToDictionary(
                record => $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", 
                record => (record.DueDate.HasValue) ? record.DueDate.Value.ToShortDateString() : "Not Yet Completed");

        Dictionary<string, string> expired = entries
            .Where(record => record.TimeToExpiry <= 0)
            .ToDictionary(
                record => $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})",
                record => (record.DueDate.HasValue) ? record.DueDate.Value.ToShortDateString() : "Not Yet Completed");

        List<EmailRecipient> warningRecipients = new() { entries.First().StaffEntry };

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