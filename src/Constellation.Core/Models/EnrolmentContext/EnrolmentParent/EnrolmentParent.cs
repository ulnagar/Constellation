namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent;

using Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Identifiers;
using EnrolmentStudent;
using ValueObjects;

public sealed class EnrolmentParent
{
    private readonly List<EnrolmentStudent> _students = [];

    /// <summary>
    /// DO NOT USE. EF CORE ONLY
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private EnrolmentParent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private EnrolmentParent(
        Guid? userId,
        Name name,
        EmailAddress email,
        PhoneNumber phoneNumber,
        MailingAddress? mailingAddress)
    {
        Id = new();

        EnrolmentUserId = userId;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        MailingAddress = mailingAddress;
    }

    public EnrolmentParentId Id { get; private set; }
    public Guid? EnrolmentUserId { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public MailingAddress? MailingAddress { get; private set; }
    public IReadOnlyList<EnrolmentStudent> Students => _students.AsReadOnly();

    public static EnrolmentParent Create(
        Guid? userId,
        Name name,
        EmailAddress? email = null,
        PhoneNumber? phoneNumber = null,
        MailingAddress? mailingAddress = null)
    {
        return new(
            userId,
            name,
            email ?? EmailAddress.None,
            phoneNumber ?? ValueObjects.PhoneNumber.Empty,
            mailingAddress);
    }

    public void UpdateUserLink(Guid? userId)
        => EnrolmentUserId = userId;

    public void UpdateParentName(Name parentName)
        => Name = parentName;

    public void UpdateParentEmail(EmailAddress parentEmail) 
        => Email = parentEmail;

    public void UpdateParentPhone(PhoneNumber parentPhone) 
        => PhoneNumber = parentPhone;

    public void UpdateMailingAddress(MailingAddress address)
        => MailingAddress = address;

    public void AddStudent(EnrolmentStudent student)
    {
        if (_students.Any(entry => entry.Id == student.Id))
            return;

        _students.Add(student);
    }

    public void RemoveStudent(EnrolmentStudent student)
        => _students.Remove(student);
}
