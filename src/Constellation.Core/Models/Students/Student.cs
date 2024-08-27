namespace Constellation.Core.Models.Students;

using Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public class Student : AggregateRoot, IAuditableEntity
{
    private readonly List<AbsenceConfiguration> _absenceConfigurations = new();
    private readonly List<SchoolEnrollment> _schoolEnrollments = new();
    private readonly List<SystemLink> _systemLinks = new();

    public StudentId StudentId { get; private set; }
    public StudentReferenceNumber StudentReferenceNumber { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public Gender Gender { get; private set; }
    public Gender PreferredGender { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    
    public readonly AwardTally AwardTally = new ();
    public IReadOnlyCollection<AbsenceConfiguration> AbsenceConfigurations => _absenceConfigurations.AsReadOnly();
    public IReadOnlyCollection<SchoolEnrollment> SchoolEnrollments => _schoolEnrollments.AsReadOnly();
    public IReadOnlyCollection<SystemLink> SystemLinks => _systemLinks.AsReadOnly();

    public static Student Create(
        string srn,
        string firstName,
        string preferredName,
        string lastName,
        string emailAddress,
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

    public void Withdraw(
        IDateTimeProvider dateTime)
    {
        IsDeleted = true;

        foreach (SchoolEnrollment entry in _schoolEnrollments.Where(entry => !entry.IsDeleted))
        {
            entry.Delete(dateTime);
        }

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