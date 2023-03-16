#nullable enable
namespace Constellation.Core.Models.Families;

using Constellation.Core.Primitives;
using Constellation.Core.ValueObjects;
using System;

public sealed class Parent : Entity
{
    private Parent(
        Guid id,
        string title,
        string firstName,
        string lastName,
        string mobileNumber,
        string emailAddress)
        : base(id)
    {
        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber;
        EmailAddress = emailAddress;
    }

    public Guid FamilyId { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string MobileNumber { get; private set; } = string.Empty;
    public string EmailAddress { get; private set; } = string.Empty;

    internal static Parent Create(
        Guid id,
        string title,
        string firstName,
        string lastName,
        string mobileNumber,
        EmailAddress emailAddress)
    {
        return new Parent(
            id,
            title,
            firstName,
            lastName,
            mobileNumber,
            emailAddress.Email);
    }

    internal void Update(
        string title,
        string firstName,
        string lastName,
        string mobileNumber,
        EmailAddress emailAddress)
    {
        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber;
        EmailAddress = emailAddress.Email;
    }
}
