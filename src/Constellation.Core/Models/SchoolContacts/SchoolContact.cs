namespace Constellation.Core.Models.SchoolContacts;

using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class SchoolContact : AggregateRoot, IAuditableEntity
{
    private readonly List<SchoolContactRole> _roles = new();

    private SchoolContact(
        string firstName,
        string lastName,
        string emailAddress,
        string phoneNumber,
        bool selfRegistered)
    {
        Id = new();

        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PhoneNumber = phoneNumber;
        SelfRegistered = selfRegistered;
    }

    public SchoolContactId Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string EmailAddress { get; private set; }
    public string PhoneNumber { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    public bool SelfRegistered { get; private set; }
    public string DisplayName => FirstName + " " + LastName;
    public IReadOnlyList<SchoolContactRole> Assignments => _roles;

    public static Result<SchoolContact> Create(
        string firstName,
        string lastName,
        string emailAddress,
        string phoneNumber,
        bool selfRegistered)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<SchoolContact>(SchoolContactErrors.Validation.FirstNameEmpty);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<SchoolContact>(SchoolContactErrors.Validation.LastNameEmpty);

        if (string.IsNullOrWhiteSpace(emailAddress))
            return Result.Failure<SchoolContact>(SchoolContactErrors.Validation.EmailAddressEmpty);

        Result<EmailAddress> email = ValueObjects.EmailAddress.Create(emailAddress);
        if (email.IsFailure)
            return Result.Failure<SchoolContact>(email.Error);

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            if (!int.TryParse(phoneNumber.Trim().Replace(" ", ""), out _))
                return Result.Failure<SchoolContact>(SchoolContactErrors.Validation.PhoneNumberInvalid);

            Result<PhoneNumber> phone = ValueObjects.PhoneNumber.Create(phoneNumber);
            if (phone.IsFailure)
                return Result.Failure<SchoolContact>(phone.Error);
        }

        SchoolContact contact = new(
            firstName,
            lastName,
            emailAddress,
            phoneNumber,
            selfRegistered);

        contact.RaiseDomainEvent(new SchoolContactCreatedDomainEvent(new(), contact.Id));

        return Result.Success(contact);
    }

    public void Delete()
    {
        foreach (SchoolContactRole role in _roles.Where(role => !role.IsDeleted))
        {
            role.Delete();

            RaiseDomainEvent(new SchoolContactRoleDeletedDomainEvent(new(), Id, role.Id));
        }

        IsDeleted = true;

        RaiseDomainEvent(new SchoolContactDeletedDomainEvent(new(), Id));
    }

    public void Reinstate()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;

        RaiseDomainEvent(new SchoolContactReinstatedDomainEvent(new(), Id));
    }

    public Result AddRole(
        string role,
        string schoolCode,
        string schoolName,
        string note)
    {
        if (string.IsNullOrWhiteSpace(role))
            return Result.Failure(SchoolContactRoleErrors.Validation.RoleEmpty);

        if (string.IsNullOrWhiteSpace(schoolCode) || string.IsNullOrWhiteSpace(schoolName))
            return Result.Failure(SchoolContactRoleErrors.Validation.SchoolCodeEmpty);
        
        SchoolContactRole contactRole = new(
            Id,
            role,
            schoolCode,
            schoolName,
            note);
        
        _roles.Add(contactRole);

        RaiseDomainEvent(new SchoolContactRoleCreatedDomainEvent(new(), Id, contactRole.Id));

        return Result.Success();
    }

    public Result RemoveRole(
        SchoolContactRoleId roleId)
    {
        SchoolContactRole role = _roles.FirstOrDefault(role => role.Id == roleId);

        if (role is null)
            return Result.Failure(SchoolContactRoleErrors.NotFound(roleId));

        role.Delete();

        RaiseDomainEvent(new SchoolContactRoleDeletedDomainEvent(new(), Id, role.Id));

        return Result.Success();
    }

    public Result UpdateRoleNote(
        SchoolContactRoleId roleId,
        string note)
    {
        SchoolContactRole role = _roles.FirstOrDefault(role => role.Id == roleId);

        if (role is null)
            return Result.Failure(SchoolContactRoleErrors.NotFound(roleId));

        role.Update(note);

        return Result.Success();
    }

    public Result Update(
        string firstName,
        string lastName,
        string emailAddress,
        string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;

        if (!string.IsNullOrWhiteSpace(emailAddress))
        {
            Result<EmailAddress> newEmail = ValueObjects.EmailAddress.Create(emailAddress);
            if (newEmail.IsFailure)
                return Result.Failure(newEmail.Error);

            if (EmailAddress != newEmail.Value.Email)
            {
                RaiseDomainEvent(new SchoolContactEmailAddressChangedDomainEvent(new(), Id, EmailAddress, newEmail.Value.Email));

                EmailAddress = newEmail.Value.Email;
            }
        }

        return Result.Success();
    }
}