namespace Constellation.Core.Models.Prospects;

using Core.Enums;
using Core.ValueObjects;
using Enums;
using Identifiers;
using Shared;
using Students.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class Student
{
    private readonly List<Parent> _parents = [];

    private Student()
    {

    }

    public ProspectiveStudentId Id { get; private set; }
    public ApplicationNumber ApplicationNumber { get; private set; } = ApplicationNumber.Empty;
    public StudentReferenceNumber StudentReferenceNumber { get; private set; } = StudentReferenceNumber.Empty;
    public Name Name { get; private set; }
    public Gender Gender { get; private set; } = Gender.Unknown;
    public Grade Grade { get; private set; }
    public string? CurrentSchoolCode { get; private set; }
    public string? CurrentSchoolName { get; private set; }
    public string DestinationSchoolCode { get; private set; }
    public string DestinationSchoolName { get; private set; }
    public Family Family { get; private set; }
    public IReadOnlyList<Parent> Parents => _parents.AsReadOnly();
    public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Unknown;


    public Result AddParent()
    {
        if (_parents.Any())
            return Result.Failure();

        _parents.Add(new());

        return Result.Success();
    }

}