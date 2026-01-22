#nullable enable
namespace Constellation.Core.Models.Families;

using Identifiers;
using ValueObjects;

public sealed class Parent
{
    private Parent() { }

    private Parent(
        FamilyId familyId,
        string title,
        Name name,
        PhoneNumber? mobileNumber,
        EmailAddress? emailAddress,
        SentralReference sentralLink)
    {
        Id = new();
        FamilyId = familyId;
        Title = title;
        Name = name;
        MobileNumber = mobileNumber ?? PhoneNumber.Empty;
        EmailAddress = emailAddress ?? EmailAddress.None;
        SentralLink = sentralLink;
    }

    public ParentId Id { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public string Title { get; private set; }
    public Name Name { get; private set; }
    public PhoneNumber MobileNumber { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public SentralReference SentralLink { get; private set; }
    public string SentralId { get; private set; } = string.Empty;

    public enum SentralReference
    {
        None,
        Mother,
        Father,
        Other
    }

    internal static Parent Create(
        FamilyId familyId,
        string title,
        Name name,
        PhoneNumber? mobileNumber,
        EmailAddress emailAddress,
        SentralReference sentralLink = SentralReference.None)
    {
        return new Parent(
            familyId,
            title,
            name,
            mobileNumber,
            emailAddress,
            sentralLink);
    }

    internal void Update(
        string title,
        Name name,
        PhoneNumber? mobileNumber,
        EmailAddress emailAddress,
        SentralReference sentralLink = SentralReference.None)
    {
        Title = title;
        Name = name;
        MobileNumber = mobileNumber;
        EmailAddress = emailAddress;
        SentralLink = sentralLink;
    }

    public void SetSentralId(string sentralId) => SentralId = sentralId;
}
