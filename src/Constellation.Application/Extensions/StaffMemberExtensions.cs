namespace Constellation.Application.Extensions;

using Core.Models.StaffMembers;
using Core.Shared;
using Core.ValueObjects;

public static class StaffMemberExtensions
{
    public static Result<EmailRecipient> GetEmailRecipient(this StaffMember member) 
        => EmailRecipient.Create(member.Name, member.EmailAddress);
}