namespace Constellation.Application.Features.MandatoryTraining.Models;

using AutoMapper.Execution;
using Constellation.Core.Common;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public class CompletionRecordExtendedDetailsDto
{
    public TrainingModuleId ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleFrequency { get; set; }

    public string StaffId { get; set; }
    public string StaffName { get; set; }
    public string StaffEmail { get; set; }
    public List<FacultyContactDto> StaffHeadTeachers { get; set; } = new();
    public List<FacultyContactDto> PrincipalContacts { get; set; } = new(); 

    public TrainingCompletionId RecordId { get; set; }
    public bool RecordNotRequired { get; set; }
    public DateTime? RecordEffectiveDate { get; set; }

    public bool IsLatest { get; set; }
    public int TimeToExpiry { get; set; }
    public DateTime? DueDate { get; set; }

    public class FacultyContactDto
    {
        public Guid FacultyId { get; set; }
        public string FacultyName { get; set; }
        public string FacultyHeadTeacherName { get; set; }
        public string FacultyHeadTeacherEmail { get; set; }

        public class Comparer : IEqualityComparer<FacultyContactDto>
        {
            public bool Equals(FacultyContactDto x, FacultyContactDto y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;

                if (x.FacultyHeadTeacherEmail == y.FacultyHeadTeacherEmail)
                    return true;

                return false;
            }

            public int GetHashCode([DisallowNull] FacultyContactDto obj)
            {
                int hashId = obj.FacultyId.GetHashCode();
                int hashName = obj.FacultyName == null ? 0 : obj.FacultyName.GetHashCode();
                int hashTeacherName = obj.FacultyHeadTeacherName == null ? 0 : obj.FacultyHeadTeacherName.GetHashCode();
                int hashTeacherEmail = obj.FacultyHeadTeacherEmail == null ? 0 : obj.FacultyHeadTeacherEmail.GetHashCode();

                return hashId ^ hashName ^ hashTeacherName ^ hashTeacherEmail;
            }
        }
    }

    public void AddModuleDetails(TrainingModule module)
    {
        ModuleId = module.Id;
        ModuleName = module.Name;
        ModuleFrequency = module.Expiry;
    }

    public void AddStaffDetails(Staff staff)
    {
        StaffId = staff.StaffId;
        StaffName = staff.DisplayName;
        StaffEmail = staff.EmailAddress;
    }

    public void AddHeadTeacherDetails(Faculty faculty, Staff headTeacher)
    {
        StaffHeadTeachers.Add(new FacultyContactDto
        {
            FacultyId = faculty.Id,
            FacultyName = faculty.Name,
            FacultyHeadTeacherName = headTeacher.DisplayName,
            FacultyHeadTeacherEmail = headTeacher.EmailAddress
        });
    }

    public void AddPrincipalDetails(SchoolContact principal, School school) 
    {
        PrincipalContacts.Add(new FacultyContactDto
        {
            FacultyName = school.Name,
            FacultyHeadTeacherName = principal.DisplayName,
            FacultyHeadTeacherEmail = principal.EmailAddress
        });
    }

    public void AddRecordDetails(TrainingCompletion record)
    {
        RecordId = record.Id;
        RecordNotRequired = record.NotRequired;
        RecordEffectiveDate = (record.NotRequired) ? record.CreatedAt : record.CompletedDate.Value;
    }

    public void CalculateExpiry()
    {
        if (ModuleId.Value == Guid.Empty || RecordId.Value == Guid.Empty)
        {
            DueDate = DateTime.Today;
            TimeToExpiry = -9999;
            return;
        }

        if (RecordNotRequired || (ModuleFrequency == TrainingModuleExpiryFrequency.OnceOff && RecordEffectiveDate.HasValue))
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