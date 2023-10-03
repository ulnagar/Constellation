namespace Constellation.Core.Models;

using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Student
{
    private readonly List<AbsenceConfiguration> _absenceConfigurations = new();

    public Student()
    {
        IsDeleted = false;
        DateEntered = DateTime.Now;

        Enrolments = new List<Enrolment>();
        Devices = new List<DeviceAllocation>();
        AdobeConnectOperations = new List<StudentAdobeConnectOperation>();
        MSTeamOperations = new List<StudentMSTeamOperation>();

        Absences = new List<Absence>();
        PartialAbsences = new List<StudentPartialAbsence>();
        WholeAbsences = new List<StudentWholeAbsence>();
    }

    public string StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PortalUsername { get; set; }
    public string AdobeConnectPrincipalId { get; set; }
    public string SentralStudentId { get; set; }
    public Grade CurrentGrade { get; set; }
    public Grade EnrolledGrade { get; set; }
    public string Gender { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateEntered { get; set; }
    public string SchoolCode { get; set; }
    public School School { get; set; }
    public bool IncludeInAbsenceNotifications { get; set; }
    public DateTime? AbsenceNotificationStartDate { get; set; }
    public string DisplayName => FirstName.Trim() + " " + LastName.Trim();
    public string EmailAddress => PortalUsername + "@education.nsw.gov.au";
    public byte[] Photo { get; set; }

    public List<StudentFamilyMembership> FamilyMemberships { get; set; } = new();
    public ICollection<Enrolment> Enrolments { get; set; }
    public ICollection<DeviceAllocation> Devices { get; set; }
    public ICollection<StudentAdobeConnectOperation> AdobeConnectOperations { get; set; }
    public ICollection<StudentMSTeamOperation> MSTeamOperations { get; set; }
    public ICollection<Absence> Absences { get; set; }
    public ICollection<StudentPartialAbsence> PartialAbsences { get; set; }
    public ICollection<StudentWholeAbsence> WholeAbsences { get; set; }
    public IReadOnlyCollection<AbsenceConfiguration> AbsenceConfigurations => _absenceConfigurations;

    public Result AddAbsenceConfiguration(AbsenceConfiguration configuration)
    {
        if (_absenceConfigurations.Any(config =>
            !config.IsDeleted &&
            config.AbsenceType == configuration.AbsenceType &&
            DoDateRangesOverlap(configuration.ScanStartDate, configuration.ScanEndDate, config.ScanStartDate, config.ScanEndDate)))
        {
            return Result.Failure(DomainErrors.Partners.Student.AbsenceConfiguration.RecordForRangeExists(configuration.ScanStartDate, configuration.ScanEndDate));
        }

        _absenceConfigurations.Add(configuration);

        return Result.Success();
    }

    public Name? GetName()
    {
        Result<Name> request = Name.Create(FirstName, string.Empty, LastName);

        if (request.IsSuccess)
            return request.Value;

        return null;
    }

    /// <summary>
    /// Compare two sets of dates, and determine whether there is any overlap between the ranges
    /// </summary>
    /// <param name="firstStart"></param>
    /// <param name="firstEnd"></param>
    /// <param name="secondStart"></param>
    /// <param name="secondEnd"></param>
    /// <returns></returns>
    private bool DoDateRangesOverlap(DateOnly firstStart, DateOnly firstEnd, DateOnly secondStart, DateOnly secondEnd)
    {
        if (firstStart == secondStart)
        {
            // Overlaps at the start of each range
            return true;
        }

        if (firstEnd == secondEnd)
        {
            // Overlaps at the end of each range
            return true;
        }

        if (firstStart < secondStart && firstEnd <= secondEnd)
        {
            // Overlaps at the start of the second range
            return true;
        }

        if (firstStart > secondStart && firstStart <= secondEnd)
        {
            // Second range encompasses first range
            return true;
        }

        if (firstStart < secondStart && firstEnd > secondEnd)
        {
            // First range encompasses second range
            return true;
        }

        return false;
    }
}