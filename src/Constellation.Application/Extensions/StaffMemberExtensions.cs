namespace Constellation.Application.Extensions;

using Core.Models.StaffMembers;
using Core.Shared;
using Core.ValueObjects;

public static class StaffMemberExtensions
{
    public static EmailRecipient GetEmailRecipient(this StaffMember member)
    {
        Result<EmailRecipient> recipient = EmailRecipient.Create(member.Name, member.EmailAddress);

        if (recipient.IsFailure)
        {
            return EmailRecipient.AuroraCollege;
        }

        return recipient.Value;
    }
}