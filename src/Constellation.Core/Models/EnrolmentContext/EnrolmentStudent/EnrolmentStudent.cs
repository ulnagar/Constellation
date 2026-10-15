namespace Constellation.Core.Models.EnrolmentContext.EnrolmentStudent;

using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using EnrolmentParent;
using Errors;
using Identifiers;
using Shared;
using System;
using ValueObjects;

public sealed class EnrolmentStudent
{
    private readonly List<EnrolmentParent> _parents = [];

    /// <summary>
    /// DO NOT USE. EF CORE ONLY
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private EnrolmentStudent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    
    private EnrolmentStudent(
        Name name,
        Gender gender,
        StudentReferenceNumber srn,
        DateOnly? dateOfBirth,
        EmailAddress email)
    {
        Id = new();

        StudentReferenceNumber = srn;
        Name = name;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Email = email;
    }

    public EnrolmentStudentId Id { get; private set; }
    public StudentReferenceNumber? StudentReferenceNumber { get; private set; }
    public Name Name { get; private set; }
    public Gender Gender { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public EmailAddress Email { get; private set; }
    public IReadOnlyList<EnrolmentParent> Parents => _parents.AsReadOnly();

    public static EnrolmentStudent Create(
        Name name,
        Gender? gender = null,
        StudentReferenceNumber? srn = null,
        DateOnly? dateOfBirth = null,
        EmailAddress? email = null)
    {
        return new(
            name,
            gender ?? Gender.Empty,
            srn ?? StudentReferenceNumber.Empty,
            dateOfBirth,
            email ?? EmailAddress.None);
    }

    public void UpdateDateOfBirth(DateOnly dateOfBirth) 
        => DateOfBirth = dateOfBirth;

    public void UpdateGender(Gender gender)
        => Gender = gender;
    
    public void UpdateStudentEmail(EmailAddress studentEmail) 
        => Email = studentEmail;
    
    public void UpdateStudentName(Name studentName) 
        => Name = studentName;

    public void UpdateStudentReferenceNumber(StudentReferenceNumber srn) 
        => StudentReferenceNumber = srn;

    public void AddParent(EnrolmentParent parent)
    {
        if (_parents.Any(entry => entry.Id == parent.Id))
            return;

        _parents.Add(parent);
    }

    public Result RemoveParent(EnrolmentParent parent)
    {
        if (_parents.All(entry => entry.Id != parent.Id))
            return Result.Success();

        if (_parents.Count == 1)
            return Result.Failure(EnrolmentStudentErrors.MustHaveOneParent);

        _parents.Remove(parent);

        return Result.Success();
    }
}
