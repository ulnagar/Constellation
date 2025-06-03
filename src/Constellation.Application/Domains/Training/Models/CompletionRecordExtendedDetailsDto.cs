namespace Constellation.Application.Domains.Training.Models;

using Core.Enums;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.SchoolContacts;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training;
using Core.Models.Training.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

public class CompletionRecordExtendedDetailsDto
{
    public TrainingModuleId? ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleFrequency { get; set; }

    public StaffId StaffId { get; set; }
    public EmailRecipient StaffEntry { get; set; }
    public List<EmailRecipient> StaffHeadTeachers { get; set; } = new();
    public List<EmailRecipient> PrincipalContacts { get; set; } = new();

    public TrainingCompletionId? RecordId { get; set; }
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

    public void AddStaffDetails(StaffMember staff)
    {
        StaffId = staff.Id;

        Result<EmailRecipient> request = EmailRecipient.Create(
            staff.Name.DisplayName,
            staff.EmailAddress.Email);

        if (request.IsFailure)
            return;

        StaffEntry = request.Value;
    }

    public void AddHeadTeacherDetails(Faculty faculty, StaffMember headTeacher)
    {
        Result<EmailRecipient> request = EmailRecipient.Create(
            headTeacher.Name.DisplayName,
            headTeacher.EmailAddress.Email);

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
        if (ModuleId is null || ModuleId.Value.Value == Guid.Empty || RecordId is null || RecordId.Value.Value == Guid.Empty)
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