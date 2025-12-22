namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetContactDetails;

using Core.ValueObjects;

public sealed record ContactDetail(
    Name Name,
    string AdditionalDetail,
    ContactDetail.ContactCategory Category,
    PhoneNumber PhoneNumber,
    EmailAddress EmailAddress)
{
    public enum ContactCategory
    {
        Staff,
        Parent,
        Coordinator
    }
}