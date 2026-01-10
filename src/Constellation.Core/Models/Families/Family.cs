namespace Constellation.Core.Models.Families;

using Constellation.Core.Models.Students.Identifiers;
using Core.Errors;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class Family : AggregateRoot, IAuditableEntity
{
    private readonly List<StudentFamilyMembership> _studentMemberships = new();
    private readonly List<Parent> _parents = new();

    private Family(
        string familyTitle)
    {
        Id = new FamilyId();
        FamilyTitle = familyTitle;
    }

    public FamilyId Id { get; }
    public string SentralId { get; private set; } = string.Empty;
    public IReadOnlyCollection<StudentFamilyMembership> Students => _studentMemberships;
    public IReadOnlyCollection<Parent> Parents => _parents;
    public string FamilyTitle { get; private set; }
    public string AddressLine1 { get; private set; } = string.Empty;
    public string AddressLine2 { get; private set; } = string.Empty;
    public string AddressTown { get; private set; } = string.Empty;
    public string AddressPostCode { get; private set; } = string.Empty;
    public string FamilyEmail { get; private set; } = string.Empty;

    public string? CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Family Create(string familyTitle)
    {
        Family family = new(familyTitle);

        return family;
    }

    public Result LinkFamilyToSentralDetails(string sentralId)
    {
        if (string.IsNullOrWhiteSpace(sentralId))
            return Result.Failure(DomainErrors.LinkedSystems.Sentral.FamilyIdNotValid(sentralId));

        SentralId = sentralId;

        return Result.Success();
    }

    public Result UpdateFamilyAddress(
        string title,
        string line1,
        string line2,
        string town,
        string postcode)
    {
        if (!string.IsNullOrWhiteSpace(title) &&
            !string.IsNullOrWhiteSpace(line1) &&
            !string.IsNullOrWhiteSpace(town) &&
            !string.IsNullOrWhiteSpace(postcode))
        {
            FamilyTitle = title;
            AddressLine1 = line1;
            AddressLine2 = line2;
            AddressTown = town;
            AddressPostCode = postcode;

            return Result.Success();
        }

        return Result.Failure(FamilyErrors.InvalidAddress);
    }

    public Result UpdateFamilyEmail(string email)
    {
        Result<EmailAddress> familyEmail = EmailAddress.Create(email);

        if (familyEmail.IsFailure)
            return Result.Failure(familyEmail.Error);

        RaiseDomainEvent(new FamilyEmailAddressChangedDomainEvent(new(), Id, FamilyEmail, familyEmail.Value.Email));

        FamilyEmail = familyEmail.Value.Email;

        return Result.Success();
    }

    public Result<Parent> AddParent(
        string title,
        string firstName,
        string lastName,
        string mobileNumber,
        string emailAddress,
        Parent.SentralReference sentralLink)
    {
        Result<EmailAddress> parentEmail = EmailAddress.Create(emailAddress);

        if (parentEmail.IsFailure)
            return Result.Failure<Parent>(parentEmail.Error);

        Result<Name> parentName = Name.Create(firstName, string.Empty, lastName);

        if (parentName.IsFailure)
            return Result.Failure<Parent>(parentName.Error);

        Result<PhoneNumber> parentMobile = PhoneNumber.Create(mobileNumber);
        
        Parent? existingParent = _parents.FirstOrDefault(parent => parent.EmailAddress == parentEmail.Value.Email && parent.SentralLink == sentralLink);

        if (existingParent is not null)
            return Result.Failure<Parent>(ParentErrors.AlreadyExists);

        Parent parent = Parent.Create(
            Id,
            title,
            parentName.Value,
            (parentMobile.IsSuccess ? parentMobile.Value : null),
            parentEmail.Value,
            sentralLink);

        RaiseDomainEvent(new ParentAddedToFamilyDomainEvent(new(), Id, parent.Id));

        _parents.Add(parent);

        return parent;
    }

    public Result<Parent> UpdateParent(
        ParentId parentId,
        string title,
        string firstName,
        string lastName,
        string mobileNumber,
        string emailAddress,
        Parent.SentralReference sentralLink)
    {
        Result<EmailAddress> parentEmail = EmailAddress.Create(emailAddress);

        if (parentEmail.IsFailure)
            return Result.Failure<Parent>(parentEmail.Error);
        
        Result<Name> parentName = Name.Create(firstName, string.Empty, lastName);

        if (parentName.IsFailure)
            return Result.Failure<Parent>(parentName.Error);
        
        Result<PhoneNumber> parentMobile = PhoneNumber.Create(mobileNumber);

        Parent? existingParent = _parents.FirstOrDefault(parent => parent.Id == parentId);

        if (existingParent is null)
            return Result.Failure<Parent>(ParentErrors.NotFoundInFamily(parentId, Id));

        if (parentEmail.Value.Email != existingParent.EmailAddress)
            RaiseDomainEvent(new ParentEmailAddressChangedDomainEvent(new(), Id, existingParent.Id, existingParent.EmailAddress, parentEmail.Value.Email));

        existingParent.Update(
            title,
            parentName.Value,
            (parentMobile.IsSuccess ? parentMobile.Value : null),
            parentEmail.Value,
            sentralLink);

        return existingParent;
    }

    public Result RemoveParent(
        ParentId parentId)
    {
        Parent? existingParent = _parents.FirstOrDefault(entry => entry.Id == parentId);

        if (existingParent is null)
            return Result.Success();

        _parents.Remove(existingParent);

        RaiseDomainEvent(new ParentRemovedFromFamilyDomainEvent(new(), Id, parentId, existingParent.EmailAddress));

        return Result.Success();
    }

    public Result<StudentFamilyMembership> AddStudent(
        StudentId studentId,
        bool isResidential)
    {
        StudentFamilyMembership? existingMembership = _studentMemberships.FirstOrDefault(entry => entry.StudentId == studentId);

        if (existingMembership is null)
        {
            StudentFamilyMembership membership = StudentFamilyMembership.Create(studentId, Id, isResidential);

            RaiseDomainEvent(new StudentAddedToFamilyDomainEvent(new(), membership));

            _studentMemberships.Add(membership);

            return membership;
        }

        if (existingMembership.IsResidentialFamily != isResidential)
        {
            existingMembership.IsResidentialFamily = isResidential;

            if (isResidential)
            {
                RaiseDomainEvent(new StudentResidentialFamilyChangedDomainEvent(new(), existingMembership));
            }
        }

        return existingMembership;
    }

    public Result RemoveStudent(
        StudentId studentId)
    {
        StudentFamilyMembership? existingMembership = _studentMemberships.FirstOrDefault(entry => entry.StudentId == studentId);

        if (existingMembership is null)
            return Result.Success();

        _studentMemberships.Remove(existingMembership);

        RaiseDomainEvent(new StudentRemovedFromFamilyDomainEvent(new(), existingMembership));

        return Result.Success();
    }

    public void Delete()
    {
        IsDeleted = true;

        RaiseDomainEvent(new FamilyDeletedDomainEvent(new(), Id));
    }

    public void Reinstate()
    {
        IsDeleted = false;
        DeletedAt = DateTime.MinValue;
        DeletedBy = string.Empty;

        RaiseDomainEvent(new FamilyReinstatedDomainEvent(new(), Id));
    }
}
