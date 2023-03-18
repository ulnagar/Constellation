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
        string emailAddress,
        SentralReference sentralLink)
        : base(id)
    {
        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber;
        EmailAddress = emailAddress;
        SentralLink = sentralLink;
    }

    public Guid FamilyId { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string MobileNumber { get; private set; } = string.Empty;
    public string EmailAddress { get; private set; } = string.Empty;
    public SentralReference SentralLink { get; private set; }

    public enum SentralReference
    {
        None,
        Mother,
        Father,
        Other
    }

    internal static Parent Create(
        Guid id,
        string title,
        string firstName,
        string lastName,
        PhoneNumber mobileNumber,
        EmailAddress emailAddress,
        SentralReference sentralLink = SentralReference.None)
    {
        return new Parent(
            id,
            title,
            firstName,
            lastName,
            mobileNumber.ToString(PhoneNumber.Format.None),
            emailAddress.Email,
            sentralLink);
    }

    internal void Update(
        string title,
        string firstName,
        string lastName,
        PhoneNumber mobileNumber,
        EmailAddress emailAddress,
        SentralReference sentralLink = SentralReference.None)
    {
        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber.ToString(PhoneNumber.Format.None);
        EmailAddress = emailAddress.Email;
        SentralLink = sentralLink;
    }
}
