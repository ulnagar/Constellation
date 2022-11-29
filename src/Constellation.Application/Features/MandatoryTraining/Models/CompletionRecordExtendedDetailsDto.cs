﻿namespace Constellation.Application.Features.MandatoryTraining.Models;

using Constellation.Core.Common;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public class CompletionRecordExtendedDetailsDto
{
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; }
    public TrainingModuleExpiryFrequency ModuleFrequency { get; set; }

    public string StaffId { get; set; }
    public string StaffName { get; set; }
    public string StaffEmail { get; set; }
    public List<FacultyContactDto> StaffHeadTeachers { get; set; } = new();
    public List<FacultyContactDto> PrincipalContacts { get; set; } = new(); 

    public Guid RecordId { get; set; }
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

        foreach (var faculty in staff.Faculties.Where(member => !member.IsDeleted && member.Role != FacultyMembershipRole.Manager).Select(member => member.Faculty))
        {
            foreach (var member in faculty.Members.Where(entry => !entry.IsDeleted && entry.Role == FacultyMembershipRole.Manager))
            {
                StaffHeadTeachers.Add(new FacultyContactDto
                {
                    FacultyId = faculty.Id,
                    FacultyName = faculty.Name,
                    FacultyHeadTeacherName = member.Staff.DisplayName,
                    FacultyHeadTeacherEmail = member.Staff.EmailAddress
                });
            }
        }

        if (staff.IsShared)
        {
            var principals = staff.School.StaffAssignments.Where(role => role.Role == SchoolContactRole.Principal && !role.IsDeleted).ToList();

            foreach (var principal in principals)
            {
                PrincipalContacts.Add(new FacultyContactDto
                {
                    FacultyName = staff.School.Name,
                    FacultyHeadTeacherName = principal.SchoolContact.DisplayName,
                    FacultyHeadTeacherEmail = principal.SchoolContact.EmailAddress
                });
            }
        }
        else
        {
            PrincipalContacts.Add(new FacultyContactDto
            {
                FacultyName = "Aurora College",
                FacultyHeadTeacherName = "Chris Robertson",
                FacultyHeadTeacherEmail = "christopher.robertson@det.nsw.edu.au"
            });
        }
    }

    public void AddRecordDetails(TrainingCompletion record)
    {
        RecordId = record.Id;
        RecordNotRequired = record.NotRequired;
        RecordEffectiveDate = (record.NotRequired) ? record.CreatedAt : record.CompletedDate.Value;
    }

    public void CalculateExpiry()
    {
        if (ModuleId == Guid.Empty || RecordId == Guid.Empty)
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