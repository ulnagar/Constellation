namespace Constellation.Core.Models.StaffMembers;

using Abstractions.Clock;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Errors;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class StaffMember : AggregateRoot, IAuditableEntity
{
    private readonly List<SchoolAssignment> _schoolAssignments = new();
    private readonly List<StaffMemberSystemLink> _systemLinks = new();

    private StaffMember() { }

    private StaffMember(
        EmployeeId employeeId,
        Name name,
        EmailAddress emailAddress,
        Gender gender,
        bool isShared)
    {
        Id = new();

        EmployeeId = employeeId;
        Name = name;
        EmailAddress = emailAddress;
        Gender = gender;
        IsShared = isShared;
    }

    public StaffId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public Gender Gender { get; private set; }
    public bool IsShared { get; private set; }


    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    public DateTime? DateDeleted { get; set; }

    public IReadOnlyCollection<SchoolAssignment> SchoolAssignments => _schoolAssignments.AsReadOnly();
    public SchoolAssignment? CurrentAssignment => _schoolAssignments
        .SingleOrDefault(entry =>
            !entry.IsDeleted &&
            entry.StartDate <= DateOnly.FromDateTime(DateTime.Today) &&
            (entry.EndDate == null || entry.EndDate >= DateOnly.FromDateTime(DateTime.Today)));

    public IReadOnlyCollection<StaffMemberSystemLink> SystemLinks => _systemLinks.AsReadOnly();


    public static Result<StaffMember> Create(
        Name name,
        Gender gender,
        bool isShared)
    {
        StaffMember entry = new(
            null,
            name,
            EmailAddress.None,
            gender,
            isShared);
        
        entry.RaiseDomainEvent(new StaffMemberCreatedDomainEvent(new(), entry.Id));

        return entry;
    }

    public static Result<StaffMember> Create(
        EmployeeId employeeId,
        Name name,
        EmailAddress emailAddress,
        School school,
        Gender gender,
        bool isShared,
        IDateTimeProvider dateTime)
    {
        if (employeeId == EmployeeId.Empty)
            return Result.Failure<StaffMember>(EmployeeIdErrors.EmptyValue);

        StaffMember entry = new(
            employeeId,
            name,
            emailAddress,
            gender,
            isShared);

        Result assignment = entry.AddSchoolAssignment(school.Code, school.Name, dateTime);

        if (assignment.IsFailure)
            return Result.Failure<StaffMember>(assignment.Error);
        
        entry.RaiseDomainEvent(new StaffMemberCreatedDomainEvent(new(), entry.Id));

        return entry;
    }

    public Result AddSchoolAssignment(
        string schoolCode,
        string schoolName,
        IDateTimeProvider dateTime,
        DateOnly? startDate = null)
    {
        startDate ??= dateTime.Today;

        SchoolAssignment? currentAssignment = CurrentAssignment;

        if (currentAssignment is not null)
        {
            if (currentAssignment.SchoolCode == schoolCode)
                return Result.Success();

            currentAssignment.Delete(startDate.Value, dateTime);
        }

        switch (currentAssignment)
        {
            case null when startDate != dateTime.Today:
                RaiseDomainEvent(new StaffMemberMovedSchoolsDomainEvent(new(), Id, string.Empty, schoolCode, startDate));
                break;
            case null when startDate == dateTime.Today:
                RaiseDomainEvent(new StaffMemberMovedSchoolsDomainEvent(new(), Id, string.Empty, schoolCode));
                break;
            case not null when startDate != dateTime.Today:
                RaiseDomainEvent(new StaffMemberMovedSchoolsDomainEvent(new(), Id, currentAssignment.SchoolCode, schoolCode, startDate));
                break;
            case not null when startDate == dateTime.Today:
                RaiseDomainEvent(new StaffMemberMovedSchoolsDomainEvent(new(), Id, currentAssignment.SchoolCode, schoolCode));
                break;
        }

        Result<SchoolAssignment> enrolment = SchoolAssignment.Create(
            Id,
            schoolCode,
            schoolName,
            startDate.Value,
            null,
            dateTime);

        if (enrolment.IsFailure)
            return Result.Failure(enrolment.Error);

        _schoolAssignments.Add(enrolment.Value);

        return Result.Success();
    }

    public Result AddSystemLink(
        SystemType type,
        string value)
    {
        StaffMemberSystemLink existingEntry = _systemLinks.FirstOrDefault(entry => entry.System == type);

        Result<StaffMemberSystemLink> entry = StaffMemberSystemLink.Create(Id, type, value);

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
        StaffMemberSystemLink existingEntry = _systemLinks.FirstOrDefault(entry => entry.System == type);

        if (existingEntry is null)
            return Result.Failure(SystemLinkErrors.NotFound(type));

        _systemLinks.Remove(existingEntry);

        return Result.Success();
    }

    public void Resign(
        IDateTimeProvider dateTime)
    {
        IsDeleted = true;

        if (CurrentAssignment is not null)
            CurrentAssignment.Delete(dateTime.Today, dateTime);

        RaiseDomainEvent(new StaffMemberResignedDomainEvent(new(), Id));
    }

    public Result Reinstate(
        School school,
        IDateTimeProvider dateTime,
        DateOnly? startDate = null)
    {
        IsDeleted = false;
        DeletedAt = DateTime.MinValue;
        DeletedBy = string.Empty;

        Result assignment = AddSchoolAssignment(
            school.Code,
            school.Name,
            dateTime,
            startDate);

        if (assignment.IsFailure)
            return Result.Failure(assignment.Error);

        RaiseDomainEvent(new StaffMemberReinstatedDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result UpdateStaffMember(
        EmployeeId employeeId,
        Name name,
        EmailAddress emailAddress,
        Gender gender,
        bool isShared)
    {
        if (EmployeeId != EmployeeId.Empty)
            EmployeeId = employeeId;

        Name = name;
        Gender = gender;

        if (EmailAddress != emailAddress)
        {
            RaiseDomainEvent(new StaffMemberEmailAddressChangedDomainEvent(new(), Id, EmailAddress.Email, emailAddress.Email));

            EmailAddress = emailAddress;
        }

        return Result.Success();
    }
}