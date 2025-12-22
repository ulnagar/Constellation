namespace Constellation.Core.Models.SchoolContacts;

using Core.Errors;
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

public sealed class SchoolContact : AggregateRoot, IAuditableEntity
{
    private readonly List<SchoolContactRole> _roles = new();

    private SchoolContact() { }
    private SchoolContact(
        Name name,
        EmailAddress emailAddress,
        PhoneNumber phoneNumber,
        bool selfRegistered)
    {
        Id = new();

        Name = name;
        EmailAddress = emailAddress;
        PhoneNumber = phoneNumber;
        SelfRegistered = selfRegistered;
    }

    public SchoolContactId Id { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    public bool SelfRegistered { get; private set; }
    public string DisplayName => Name.DisplayName;
    public IReadOnlyList<SchoolContactRole> Assignments => _roles;

    public static Result<SchoolContact> Create(
        string firstName,
        string lastName,
        string emailAddress,
        string number,
        bool selfRegistered)
    {
        Result<Name> name = Name.Create(firstName, string.Empty, lastName);
        if (name.IsFailure)
            return Result.Failure<SchoolContact>(name.Error);

        Result<EmailAddress> email = EmailAddress.Create(emailAddress);
        if (email.IsFailure)
            return Result.Failure<SchoolContact>(email.Error);

        PhoneNumber phoneNumber = PhoneNumber.Empty;

        if (!string.IsNullOrWhiteSpace(number))
        {
            if (!int.TryParse(number.Trim().Replace(" ", ""), out _))
                return Result.Failure<SchoolContact>(SchoolContactErrors.Validation.PhoneNumberInvalid);

            Result<PhoneNumber> phone = PhoneNumber.Create(number);
            if (phone.IsFailure)
                return Result.Failure<SchoolContact>(phone.Error);

            phoneNumber = phone.Value;
        }

        SchoolContact contact = new(
            name.Value,
            email.Value,
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
        Position role,
        string schoolCode,
        string schoolName,
        string note)
    {
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
    public Result AddPhoneNumber(
        PhoneNumber phoneNumber)
    {
        if (!phoneNumber.IsMobile())
            return Result.Failure(DomainErrors.ValueObjects.PhoneNumber.NumberInvalid);

        PhoneNumber = phoneNumber;
        return Result.Success();
    }

    public Result Update(
        string firstName,
        string lastName,
        string emailAddress,
        string number)
    {
        Result<Name> name = Name.Create(firstName, string.Empty, lastName);
        if (name.IsFailure)
            return Result.Failure(name.Error);

        Name = name.Value;

        PhoneNumber phoneNumber = PhoneNumber.Empty;

        if (!string.IsNullOrWhiteSpace(number))
        {
            if (!int.TryParse(number.Trim().Replace(" ", ""), out _))
                return Result.Failure<SchoolContact>(SchoolContactErrors.Validation.PhoneNumberInvalid);

            Result<PhoneNumber> phone = PhoneNumber.Create(number);
            if (phone.IsFailure)
                return Result.Failure<SchoolContact>(phone.Error);

            phoneNumber = phone.Value;
        }

        PhoneNumber = phoneNumber;

        if (!string.IsNullOrWhiteSpace(emailAddress))
        {
            Result<EmailAddress> newEmail = EmailAddress.Create(emailAddress);
            if (newEmail.IsFailure)
                return Result.Failure(newEmail.Error);

            if (EmailAddress != newEmail.Value)
            {
                RaiseDomainEvent(new SchoolContactEmailAddressChangedDomainEvent(new(), Id, EmailAddress.Email, newEmail.Value.Email));

                EmailAddress = newEmail.Value;
            }
        }

        return Result.Success();
    }
    
    public Result<EmailRecipient> GetEmailRecipient() => 
        EmailRecipient.Create(Name, EmailAddress);

}