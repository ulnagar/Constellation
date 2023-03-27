#nullable enable
namespace Constellation.Core.Models.Families;

using Constellation.Core.DomainEvents;
using Constellation.Core.Primitives;
using Constellation.Core.ValueObjects;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;

public sealed class Family : AggregateRoot, IAuditableEntity
{
    private readonly List<StudentFamilyMembership> _studentMemberships = new();
    private readonly List<Parent> _parents = new();

    private Family(
        FamilyId id,
        string familyTitle)
    {
        Id = id;
        FamilyTitle = familyTitle;
    }

    public FamilyId Id { get; private set; }
    public string SentralId { get; private set; } = string.Empty;
    public IReadOnlyCollection<StudentFamilyMembership> Students => _studentMemberships;
    public IReadOnlyCollection<Parent> Parents => _parents;
    public string FamilyTitle { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string AddressLine2 { get; private set; } = string.Empty;
    public string AddressTown { get; private set; } = string.Empty;
    public string AddressPostCode { get; private set; } = string.Empty;
    public string FamilyEmail { get; private set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Family Create(FamilyId id, string familyTitle)
    {
        Family family = new(id, familyTitle);

        return family;
    }

    public Result LinkFamilyToSentralDetails(string sentralId)
    {
        if (string.IsNullOrWhiteSpace(sentralId))
        {
            return Result.Failure(DomainErrors.LinkedSystems.Sentral.FamilyIdNotValid(sentralId));
        }

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

        return Result.Failure(DomainErrors.Families.Family.InvalidAddress);
    }

    public Result UpdateFamilyEmail(string email)
    {
        var familyEmail = EmailAddress.Create(email);

        if (familyEmail.IsFailure)
        {
            return Result.Failure(familyEmail.Error);
        }

        RaiseDomainEvent(new FamilyEmailAddressChangedDomainEvent(new DomainEventId(Guid.NewGuid()), Id, FamilyEmail, familyEmail.Value.Email));

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
        var parentEmail = EmailAddress.Create(emailAddress);

        if (parentEmail.IsFailure)
        {
            return Result.Failure<Parent>(parentEmail.Error);
        }

        var parentMobile = PhoneNumber.Create(mobileNumber);

        if (parentMobile.IsFailure)
        {
            return Result.Failure<Parent>(parentMobile.Error);
        }

        var existingParent = _parents.FirstOrDefault(parent => parent.EmailAddress == parentEmail.Value.Email);

        if (existingParent is null)
        {
            var parent = Parent.Create(
                new ParentId(Guid.NewGuid()),
                Id,
                title,
                firstName,
                lastName,
                parentMobile.Value,
                parentEmail.Value,
                sentralLink);

            RaiseDomainEvent(new ParentAddedToFamilyDomainEvent(new DomainEventId(Guid.NewGuid()), Id, parent.Id));

            _parents.Add(parent);

            return parent;
        }

        existingParent.Update(
            title,
            firstName,
            lastName,
            parentMobile.Value,
            parentEmail.Value,
            sentralLink);

        return existingParent;        
    }

    public void RemoveParent(Parent parent)
    {
        if (_parents.Any(entry => entry.Id == parent.Id))
        {
            _parents.Remove(parent);

            RaiseDomainEvent(new ParentRemovedFromFamilyDomainEvent(new DomainEventId(Guid.NewGuid()), Id, parent.Id));
        }
    }

    public Result<StudentFamilyMembership> AddStudent(
        string studentId,
        bool isResidential)
    {
        var existingMembership = _studentMemberships.FirstOrDefault(entry => entry.StudentId == studentId);

        if (existingMembership is null)
        {
            var membership = StudentFamilyMembership.Create(studentId, Id, isResidential);

            RaiseDomainEvent(new StudentAddedToFamilyDomainEvent(new DomainEventId(Guid.NewGuid()), membership));

            _studentMemberships.Add(membership);

            return membership;
        }

        if (existingMembership.IsResidentialFamily != isResidential)
        {
            existingMembership.IsResidentialFamily = isResidential;

            if (isResidential)
            {
                RaiseDomainEvent(new StudentResidentialFamilyChangedDomainEvent(new DomainEventId(Guid.NewGuid()), existingMembership));
            }
        }

        return existingMembership;
    }

    public Result RemoveStudent(
        string studentId)
    {
        var existingMembership = _studentMemberships.FirstOrDefault(entry => entry.StudentId == studentId);

        if (existingMembership is null)
        {
            return Result.Success();
        }

        _studentMemberships.Remove(existingMembership);

        RaiseDomainEvent(new StudentRemovedFromFamilyDomainEvent(new DomainEventId(Guid.NewGuid()), existingMembership));

        return Result.Success();
    }

    public void Delete()
    {
        IsDeleted = true;

        foreach (var parent in Parents)
        {
            RemoveParent(parent);
        }

        foreach (var student in Students)
        {
            RemoveStudent(student.StudentId);
        }
    }
}
