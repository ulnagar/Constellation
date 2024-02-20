namespace Constellation.Core.Models;

using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using ValueObjects;

public sealed class SchoolContact : AggregateRoot
{
    private readonly List<SchoolContactRole> _roles = new();

    private SchoolContact(
        string firstName,
        string lastName,
        string emailAddress,
        string phoneNumber,
        bool selfRegistered)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PhoneNumber = phoneNumber;
        SelfRegistered = selfRegistered;

        IsDeleted = false;
        DateEntered = DateTime.Now;
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string EmailAddress { get; private set; }
    public string PhoneNumber { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DateDeleted { get; private set; }
    public DateTime? DateEntered { get; private set; }
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

        contact.RaiseDomainEvent(new SchoolContactCreatedDomainEvent(new(), ))

        return Result.Success(contact);
    }
}