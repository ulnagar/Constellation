namespace Constellation.Application.Domains.Contacts.Queries.GetContactListForParentPortal;

public sealed record StudentSupportContactResponse(
    string FirstName,
    string LastName,
    string DisplayName,
    string EmailAddress,
    string PhoneNumber,
    string Category,
    string Detail)
{
    public static StudentSupportContactResponse GetDefault =>
        new StudentSupportContactResponse(
            string.Empty,
            string.Empty,
            "Administration Office",
            "auroracoll-h.school@det.nsw.edu.au",
            "1300 287 629",
            "Aurora College",
            string.Empty);

    public static StudentSupportContactResponse GetSupport =>
        new StudentSupportContactResponse(
            string.Empty,
            string.Empty,
            "Technology Support Team",
            "support@aurora.nsw.edu.au",
            "1300 610 733",
            "Aurora College",
            string.Empty);
}
