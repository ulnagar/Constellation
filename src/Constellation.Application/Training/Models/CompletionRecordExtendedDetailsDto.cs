namespace Constellation.Application.Training.Models;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.Faculties;
using System;
using System.Collections.Generic;
using System.Linq;

public class CompletionRecordExtendedDetailsDto
{
    public TrainingModuleId ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleFrequency { get; set; }
    public List<string> RequiredByRoles { get; set; }

    public string StaffId { get; set; }
    public EmailRecipient StaffEntry { get; set; }
    public List<EmailRecipient> StaffHeadTeachers { get; set; } = new();
    public List<EmailRecipient> PrincipalContacts { get; set; } = new();

    public TrainingCompletionId RecordId { get; set; }
    public DateTime? RecordEffectiveDate { get; set; }

    public bool IsLatest { get; set; }
    public int TimeToExpiry { get; set; }
    public DateTime? DueDate { get; set; }

    public void AddModuleDetails(TrainingModule module)
    {
        ModuleId = module.Id;
        ModuleName = module.Name;
        ModuleFrequency = module.Expiry;
    }

    public void AddStaffDetails(Staff staff)
    {
        StaffId = staff.StaffId;

        Result<EmailRecipient> request = EmailRecipient.Create(
            staff.DisplayName,
            staff.EmailAddress);

        if (request.IsFailure)
            return;

        StaffEntry = request.Value;
    }

    public void AddHeadTeacherDetails(Faculty faculty, Staff headTeacher)
    {
        Result<EmailRecipient> request = EmailRecipient.Create(
            headTeacher.DisplayName,
            headTeacher.EmailAddress);

        if (request.IsFailure)
            return;

        if (StaffHeadTeachers.All(entry => entry != request.Value))
            StaffHeadTeachers.Add(request.Value);
    }

    public void AddPrincipalDetails(SchoolContact principal, School school)
    {
        Result<EmailRecipient> request = EmailRecipient.Create(
            principal.DisplayName,
            principal.EmailAddress);

        if (request.IsFailure)
            return;

        if (PrincipalContacts.All(entry => entry != request.Value))
            PrincipalContacts.Add(request.Value);
    }

    public void AddRecordDetails(TrainingCompletion record)
    {
        RecordId = record.Id;
        RecordEffectiveDate = record.CompletedDate.ToDateTime(TimeOnly.MinValue);
    }

    public void CalculateExpiry() 
    {
        if (ModuleId is null || ModuleId.Value == Guid.Empty || RecordId is null || RecordId.Value == Guid.Empty)
        {
            DueDate = DateTime.Today;
            TimeToExpiry = -9999;
            return;
        }

        if (ModuleFrequency == TrainingModuleExpiryFrequency.OnceOff && RecordEffectiveDate.HasValue)
        {
            DueDate = null;
            TimeToExpiry = 9999;
        }
        else if (!RecordEffectiveDate.HasValue)
        {
            DueDate = DateTime.Today;
            TimeToExpiry = -9999;
        }
        else
        {
            DueDate = RecordEffectiveDate.Value.AddYears((int)ModuleFrequency);
            TimeToExpiry = (int)DueDate.Value.Subtract(DateTime.Now).TotalDays;
        }
    }
}