namespace Constellation.Core.Models.Students;

using Absences;
using Abstractions.Clock;
using Enrolments;
using Enums;
using Errors;
using Events;
using Families;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public class Student : AggregateRoot
{
    private readonly List<AbsenceConfiguration> _absenceConfigurations = new();

    private Student(
        string studentId,
        string firstName,
        string lastName,
        string portalUsername,
        Grade grade,
        string schoolCode,
        string gender)
    {
        StudentId = studentId;
        FirstName = firstName;
        LastName = lastName;
        PortalUsername = portalUsername;
        CurrentGrade = grade;
        EnrolledGrade = grade;
        Gender = gender;
        SchoolCode = schoolCode;

        DateEntered = DateTime.Now;
    }

    public Student()
    {
        IsDeleted = false;
        DateEntered = DateTime.Now;

        Enrolments = new List<Enrolment>();
        Devices = new List<DeviceAllocation>();
        MSTeamOperations = new List<StudentMSTeamOperation>();

        Absences = new List<Absence>();
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

    public readonly AwardTally AwardTally = new ();
    public List<StudentFamilyMembership> FamilyMemberships { get; set; } = new();
    public ICollection<Enrolment> Enrolments { get; set; }
    public ICollection<DeviceAllocation> Devices { get; set; }
    public ICollection<StudentMSTeamOperation> MSTeamOperations { get; set; }
    public ICollection<Absence> Absences { get; set; }
    public IReadOnlyCollection<AbsenceConfiguration> AbsenceConfigurations => _absenceConfigurations;

    public static Student Create(
        string studentId,
        string firstName,
        string lastName,
        string portalUsername,
        Grade grade,
        string schoolCode,
        string gender)
    {
        Student entry = new(
            studentId,
            firstName,
            lastName,
            portalUsername,
            grade,
            schoolCode,
            gender);

        entry.RaiseDomainEvent(new StudentCreatedDomainEvent(new(), studentId));

        return entry;
    }

    public Result AddAbsenceConfiguration(AbsenceConfiguration configuration)
    {
        if (_absenceConfigurations.Any(config =>
            !config.IsDeleted &&
            config.AbsenceType == configuration.AbsenceType &&
            DoDateRangesOverlap(configuration.ScanStartDate, configuration.ScanEndDate, config.ScanStartDate, config.ScanEndDate)))
        {
            return Result.Failure(AbsenceConfigurationErrors.RecordForRangeExists(configuration.ScanStartDate, configuration.ScanEndDate));
        }

        _absenceConfigurations.Add(configuration);

        return Result.Success();
    }

    public Name GetName()
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

        if (firstStart < secondStart && firstEnd >= secondStart)
        {
            // Overlaps at the start of the second range
            return true;
        }

        if (firstStart > secondStart && firstEnd <= secondEnd)
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

    public void Withdraw()
    {
        IsDeleted = true;
        DateDeleted = DateTime.Now;

        RaiseDomainEvent(new StudentWithdrawnDomainEvent(new(), StudentId));
    }

    public void Reinstate(IDateTimeProvider dateTime)
    {
        int yearLeft = DateDeleted!.Value.Year;
        int previousGrade = (int)CurrentGrade;
        int thisYear = dateTime.Today.Year;
        int difference = thisYear - yearLeft;
        int thisGrade = previousGrade + difference;
        
        if (thisGrade > 12 || thisGrade == previousGrade)
        {
            // Do NOTHING!
        }
        else if (Enum.IsDefined(typeof(Grade), thisGrade))
        {
            Grade newGrade = (Grade)thisGrade;
            CurrentGrade = newGrade;
        }

        IsDeleted = false;
        DateDeleted = null;

        RaiseDomainEvent(new StudentReinstatedDomainEvent(new(), StudentId));
    }

    public Result UpdateStudent(
        string firstName,
        string lastName,
        string portalUsername,
        string adobeConnectId,
        string sentralId,
        Grade currentGrade,
        Grade enrolledGrade,
        string gender,
        string schoolCode)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(StudentErrors.FirstNameInvalid);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure(StudentErrors.LastNameInvalid);

        if (string.IsNullOrWhiteSpace(portalUsername))
            return Result.Failure(StudentErrors.PortalUsernameInvalid);

        if (string.IsNullOrWhiteSpace(gender) ||
            (gender != "M" && gender != "F"))
            return Result.Failure(StudentErrors.GenderInvalid);

        if (string.IsNullOrWhiteSpace(schoolCode) || 
            schoolCode.Length != 4)
            return Result.Failure(StudentErrors.SchoolCodeInvalid);

        FirstName = firstName;
        LastName = lastName;
        PortalUsername = portalUsername;
        AdobeConnectPrincipalId = adobeConnectId;
        SentralStudentId = sentralId;
        CurrentGrade = currentGrade;
        EnrolledGrade = enrolledGrade;
        Gender = gender;

        if (SchoolCode != schoolCode)
        {
            RaiseDomainEvent(new StudentMovedSchoolsDomainEvent(new(), StudentId, SchoolCode, schoolCode));

            SchoolCode = schoolCode;
        }

        return Result.Success();
    }
}