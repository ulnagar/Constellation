namespace Constellation.Core.Models.StaffMembers;

using Abstractions.Clock;
using Constellation.Core.Errors;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;

public sealed class SchoolAssignment : IAuditableEntity
{
    private SchoolAssignment() { }

    private SchoolAssignment(
        StaffId staffId,
        string schoolCode,
        string schoolName,
        DateOnly startDate,
        DateOnly? endDate)
    {
        Id = new();
        StaffId = staffId;
        SchoolCode = schoolCode;
        SchoolName = schoolName;
        StartDate = startDate;
        EndDate = endDate;
    }

    public SchoolAssignmentId Id { get; private set; }
    public StaffId StaffId { get; private set; }
    public string SchoolCode { get; private set; }
    public string SchoolName { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal static Result<SchoolAssignment> Create(
        StaffId staffId,
        string schoolCode,
        string schoolName,
        DateOnly? startDate,
        DateOnly? endDate,
        IDateTimeProvider dateTime)
    {
        if (staffId == StaffId.Empty)
            return Result.Failure<SchoolAssignment>(StaffMemberErrors.InvalidId);

        if (string.IsNullOrWhiteSpace(schoolCode) || string.IsNullOrWhiteSpace(schoolName))
            return Result.Failure<SchoolAssignment>(DomainErrors.Partners.School.NotFound(schoolCode));

        startDate ??= dateTime.Today;
        
        return new SchoolAssignment(
            staffId,
            schoolCode,
            schoolName,
            startDate.Value,
            endDate);
    }

    internal void Delete(
        DateOnly endDate,
        IDateTimeProvider dateTime)
    {
        if (endDate <= dateTime.Today)
            IsDeleted = true;
        
        EndDate = endDate;
    }
}