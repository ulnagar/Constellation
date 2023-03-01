namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class MandatoryTrainingReminderJob : IMandatoryTrainingReminderJob
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public MandatoryTrainingReminderJob(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        var detailedRecords = new List<CompletionRecordExtendedDetailsDto>();

        // Get the list of overdue completions

        // - Get all modules
        var modules = await _context.MandatoryTraining.Modules
            .Where(module => !module.IsDeleted)
            //.AsNoTracking()
            .ToListAsync(token);

        // - Get all staff
        var staff = await _context.Staff
            .Include(staff => staff.Faculties)
            .ThenInclude(member => member.Faculty)
            .ThenInclude(faculty => faculty.Members)
            .Include(staff => staff.School)
            .ThenInclude(school => school.StaffAssignments)
            .ThenInclude(role => role.SchoolContact)
            .Where(staff => !staff.IsDeleted)
            //.AsNoTracking()
            .ToListAsync(token);

        // - Get all completions
        var records = await _context.MandatoryTraining.CompletionRecords
            .Where(record => !record.IsDeleted)
            //.AsNoTracking()
            .ToListAsync(token);

        // - Build a collection of completions by each staff member for each module
        foreach (var record in records)
        {
            var entry = new CompletionRecordExtendedDetailsDto();
            entry.AddRecordDetails(record);

            entry.AddModuleDetails(record.Module);

            entry.AddStaffDetails(record.Staff);

            detailedRecords.Add(entry);
        }

        // - If a staff member has not completed a module, create a blank entry for them
        foreach (var module in modules)
        {
            var staffIds = staff.Select(staff => staff.StaffId).ToList();
            var staffRecords = detailedRecords.Where(record => record.ModuleId == module.Id).Select(record => record.StaffId).ToList();

            var missingStaffIds = staffIds.Except(staffRecords).ToList();

            foreach (var staffId in missingStaffIds)
            {
                var entry = new CompletionRecordExtendedDetailsDto();
                entry.AddModuleDetails(module);

                var staffMember = staff.First(member => member.StaffId == staffId);
                entry.AddStaffDetails(staffMember);

                entry.RecordEffectiveDate = DateTime.MinValue;
                entry.RecordNotRequired = false;

                detailedRecords.Add(entry);
            }
        }

        // - Remove all superceded entries
        foreach (var record in detailedRecords)
        {
            var duplicates = detailedRecords.Where(entry =>
                    entry.ModuleId == record.ModuleId &&
                    entry.StaffId == record.StaffId &&
                    entry.RecordId != record.RecordId)
                .ToList();

            if (duplicates.All(entry => entry.RecordEffectiveDate < record.RecordEffectiveDate))
            {
                record.IsLatest = true;
                record.CalculateExpiry();
            }
        }

        // Remove entries for staff who are no longer active
        var recordStaff = detailedRecords.Select(record => record.StaffId).Distinct().ToList();
        foreach (var staffId in recordStaff)
        {
            if (staff.All(member => member.StaffId != staffId))
            {
                detailedRecords.RemoveAll(record => record.StaffId == staffId);
            }
        }

        // - Remove all entries that are not due for expiry within 30 days, or have not already expired
        var latestRecords = detailedRecords.Where(record => record.IsLatest && record.TimeToExpiry <= 30).ToList();

        // Group by staff member
        // - Send emails to each staff member
        foreach (var teacher in latestRecords.GroupBy(record => record.StaffEmail))
        {
            await SendEmail(teacher.ToList());
        }
    }

    public async Task SendEmail(List<CompletionRecordExtendedDetailsDto> entries)
    {
        var warning = entries
            .Where(record => record.TimeToExpiry <= 30 && record.TimeToExpiry > 14)
            .Select(record => new { Name = $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", Date = record.DueDate.Value.ToShortDateString() })
            .ToDictionary(record => record.Name, record => record.Date);

        var alert = entries
            .Where(record => record.TimeToExpiry <= 14 && record.TimeToExpiry > 0)
            .Select(record => new { Name = $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", Date = record.DueDate.Value.ToShortDateString() })
            .ToDictionary(record => record.Name, record => record.Date);

        var expired = entries.Where(record => record.TimeToExpiry <= 0)
            .Select(record => new { Name = $"{record.ModuleName} ({record.ModuleFrequency.GetDisplayName()})", Date = record.DueDate.Value.ToShortDateString() })
            .ToDictionary(record => record.Name, record => record.Date);

        var warningRecipients = new Dictionary<string, string> { { entries.First().StaffName, entries.First().StaffEmail } };

        var alertRecipients = new Dictionary<string, string>(warningRecipients);
        var headTeachers = entries.SelectMany(entry => entry.StaffHeadTeachers).Distinct(new CompletionRecordExtendedDetailsDto.FacultyContactDto.Comparer()).ToList();
        foreach (var teacher in headTeachers)
        {
            if (alertRecipients.All(entry => entry.Key != teacher.FacultyHeadTeacherName))
                alertRecipients.Add(teacher.FacultyHeadTeacherName, teacher.FacultyHeadTeacherEmail);
        }

        var expiredRecipients = new Dictionary<string, string>(alertRecipients);
        if (expiredRecipients.All(entry => entry.Key != "Cathy Crouch"))
            expiredRecipients.Add("Cathy Crouch", "catherine.crouch@det.nsw.edu.au");

        foreach (var principal in entries.First().PrincipalContacts)
        {
            if (expiredRecipients.All(entry => entry.Key != principal.FacultyHeadTeacherName))
                expiredRecipients.Add(principal.FacultyHeadTeacherName, principal.FacultyHeadTeacherEmail);
        }

        if (warning.Count > 0)
            await _emailService.SendTrainingExpiryWarningEmail(warning, warningRecipients);

        if (alert.Count > 0)
            await _emailService.SendTrainingExpiryAlertEmail(alert, alertRecipients);

        if (expired.Count > 0)
            await _emailService.SendTrainingExpiredEmail(expired, expiredRecipients);
    }
}