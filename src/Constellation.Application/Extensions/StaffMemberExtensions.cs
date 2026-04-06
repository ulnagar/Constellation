namespace Constellation.Application.Extensions;

using Core.Models.StaffMembers;
using Core.Shared;
using Core.ValueObjects;

public static class StaffMemberExtensions
{
    extension(StaffMember member)
    {
        public Result<EmailRecipient> GetEmailRecipient 
            => EmailRecipient.Create(member.Name, member.EmailAddress);

        public Result<SmsRecipient> GetSmsRecipient
            => SmsRecipient.Create(member.Name, member.PhoneNumber);
    }
}