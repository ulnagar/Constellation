namespace Constellation.Application.Extensions;

using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;

public static class StudentExtensions
{
    public static Result<EmailRecipient> GetEmailRecipient(this Student student) 
        => EmailRecipient.Create(student.Name, student.EmailAddress);
}