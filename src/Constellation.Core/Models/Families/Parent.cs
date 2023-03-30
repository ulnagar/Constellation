#nullable enable
namespace Constellation.Core.Models.Families;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;

public sealed class Parent
{
    private Parent(
        ParentId id,
        FamilyId familyId,
        string title,
        string firstName,
        string lastName,
        string mobileNumber,
        string emailAddress,
        SentralReference sentralLink)
    {
        Id = id;
        FamilyId = familyId;
        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MobileNumber = mobileNumber;
        EmailAddress = emailAddress;
        SentralLink = sentralLink;
    }

    public ParentId Id { get; private set; }
    public FamilyId FamilyId { get; private set; }
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
        ParentId id,
        FamilyId familyId,
        string title,
        string firstName,
        string lastName,
        PhoneNumber? mobileNumber,
        EmailAddress emailAddress,
        SentralReference sentralLink = SentralReference.None)
    {
        var number = (mobileNumber is not null ? mobileNumber.ToString(PhoneNumber.Format.None) : string.Empty);

        return new Parent(
            id,
            familyId,
            title,
            firstName,
            lastName,
            number,
            emailAddress.Email,
            sentralLink);
    }

    internal void Update(
        string title,
        string firstName,
        string lastName,
        PhoneNumber? mobileNumber,
        EmailAddress emailAddress,
        SentralReference sentralLink = SentralReference.None)
    {
        Title = title;
        FirstName = firstName;
        LastName = lastName;

        if (mobileNumber is not null)
            MobileNumber = mobileNumber.ToString(PhoneNumber.Format.None);
        else
            MobileNumber = string.Empty;

        EmailAddress = emailAddress.Email;
        SentralLink = sentralLink;
    }
}
