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
    private readonly List<SchoolEnrolment> _schoolEnrolments = new();
    private readonly List<SystemLink> _systemLinks = new();

    private Student() { }

    private Student(
        StudentReferenceNumber studentReferenceNumber,
        Name name,
        EmailAddress emailAddress,
        Gender gender,
        Gender? preferredGender)
    {
        Id = new();

        StudentReferenceNumber = studentReferenceNumber;
        Name = name;
        EmailAddress = emailAddress;
        Gender = gender;
        PreferredGender = preferredGender ?? gender;
    }

    public StudentId Id { get; private set; }
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
    
    public AwardTally AwardTally { get; private set; }
    public IReadOnlyCollection<AbsenceConfiguration> AbsenceConfigurations => _absenceConfigurations.AsReadOnly();
    public IReadOnlyCollection<SchoolEnrolment> SchoolEnrolments => _schoolEnrolments.AsReadOnly();

    public SchoolEnrolment? CurrentEnrolment => _schoolEnrolments
        .SingleOrDefault(entry => 
            !entry.IsDeleted &&
            entry.StartDate <= DateOnly.FromDateTime(DateTime.Today) &&
            (entry.EndDate == null || entry.EndDate >= DateOnly.FromDateTime(DateTime.Today)));

    public IReadOnlyCollection<SystemLink> SystemLinks => _systemLinks.AsReadOnly();

    public static Result<Student> Create(
        Name name,
        Gender gender,
        IDateTimeProvider dateTime)
    {
        Student entry = new(
            null,
            name,
            EmailAddress.None,
            gender,
            null);

        entry.AwardTally = new(entry.Id);

        entry.RaiseDomainEvent(new StudentCreatedDomainEvent(new(), entry.Id));

        return entry;
    }

    public static Result<Student> Create(
        StudentReferenceNumber srn,
        Name name,
        EmailAddress emailAddress,
        Grade grade,
        School school,
        Gender gender,
        IDateTimeProvider dateTime)
    {
        if (srn == StudentReferenceNumber.Empty)
            return Result.Failure<Student>(StudentReferenceNumberErrors.EmptyValue);

        Student entry = new(
            srn,
            name,
            emailAddress,
            gender,
            null);

        Result enrolment = entry.AddSchoolEnrolment(school.Code, school.Name, grade, dateTime);

        if (enrolment.IsFailure)
            return Result.Failure<Student>(enrolment.Error);

        entry.AwardTally = new(entry.Id);

        entry.RaiseDomainEvent(new StudentCreatedDomainEvent(new(), entry.Id));

        return entry;
    }

    public Result AddSchoolEnrolment(
        string schoolCode,
        string schoolName,
        Grade grade,
        IDateTimeProvider dateTime,
        DateOnly? startDate = null)
    {
        startDate ??= dateTime.Today;

        SchoolEnrolment? currentEnrolment = CurrentEnrolment;

        if (currentEnrolment is not null)
        {
            if (currentEnrolment.SchoolCode == schoolCode &&
                currentEnrolment.Grade == grade)
                return Result.Success();

            if (currentEnrolment.SchoolCode != schoolCode)
                RaiseDomainEvent(new StudentMovedSchoolsDomainEvent(new(), Id, currentEnrolment.SchoolCode, schoolCode, startDate));

            currentEnrolment.Delete(startDate.Value, dateTime);
        }

        Result<SchoolEnrolment> enrolment = SchoolEnrolment.Create(
            Id,
            schoolCode,
            schoolName,
            grade,
            startDate.Value.Year,
            startDate.Value,
            null,
            dateTime);

        if (enrolment.IsFailure)
            return Result.Failure(enrolment.Error);

        _schoolEnrolments.Add(enrolment.Value);
        
        return Result.Success();
    }

    public Result AddSystemLink(
        SystemType type,
        string value)
    {
        SystemLink existingEntry = _systemLinks.FirstOrDefault(entry => entry.System == type);

        Result<SystemLink> entry = SystemLink.Create(Id, type, value);

        if (entry.IsFailure)
            return Result.Failure(entry.Error);

        if (existingEntry is not null)
            _systemLinks.Remove(existingEntry);

        _systemLinks.Add(entry.Value);

        return Result.Success();
    }

    public Result RemoveSystemLink(
        SystemType type)
    {
        SystemLink existingEntry = _systemLinks.FirstOrDefault(entry => entry.System == type);

        if (existingEntry is null)
            return Result.Failure(SystemLinkErrors.NotFound(type));

        _systemLinks.Remove(existingEntry);

        return Result.Success();
    }

    public Result AddAbsenceConfiguration(AbsenceConfiguration configuration)
    {
        List<AbsenceConfiguration> existingEntry = _absenceConfigurations.Where(config =>
                !config.IsDeleted &&
                config.AbsenceType == configuration.AbsenceType &&
                DoDateRangesOverlap(
                    configuration.ScanStartDate, 
                    configuration.ScanEndDate, 
                    config.ScanStartDate,
                config.ScanEndDate))
            .ToList();

        if (existingEntry.Count > 0)
            return Result.Failure(AbsenceConfigurationErrors.RecordForRangeExists(configuration.ScanStartDate, configuration.ScanEndDate));

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

        if (CurrentEnrolment is not null)
            CurrentEnrolment.Delete(dateTime.Today, dateTime);
     
        RaiseDomainEvent(new StudentWithdrawnDomainEvent(new(), Id));
    }

    public Result Reinstate(
        School school,
        Grade grade,
        IDateTimeProvider dateTime,
        DateOnly? startDate = null)
    {
        IsDeleted = false;
        DeletedAt = DateTime.MinValue;
        DeletedBy = string.Empty;

        Result enrolment = AddSchoolEnrolment(
            school.Code, 
            school.Name,
            grade,
            dateTime,
            startDate);

        if (enrolment.IsFailure)
            return Result.Failure(enrolment.Error);

        RaiseDomainEvent(new StudentReinstatedDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result UpdateStudent(
        StudentReferenceNumber srn,
        Name name,
        EmailAddress emailAddress,
        Gender preferredGender)
    {
        if (srn != StudentReferenceNumber.Empty)
            StudentReferenceNumber = srn;

        Name = name;
        PreferredGender = preferredGender;

        if (EmailAddress != emailAddress)
        {
            RaiseDomainEvent(new StudentEmailAddressChangedDomainEvent(new(), Id, EmailAddress, emailAddress));

            EmailAddress = emailAddress;
        }

        return Result.Success();
    }
}