namespace Constellation.Core.ValueObjects;

using Primitives;

public sealed class EmailSender : ValueObject
{
    private EmailSender() { }

    private EmailSender(string name, string email)
    {
        Name = name;
        Email = email;
    }
    
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }

    // Implicit conversion from EmailRecipient — replaces the From() factory method
    public static implicit operator EmailSender(EmailRecipient recipient) =>
        new(recipient.Name, recipient.Email);

    // Implicit conversion back to EmailRecipient — useful if you need to pass
    // an EmailSender to something expecting an EmailRecipient
    public static implicit operator EmailRecipient(EmailSender sender) =>
        EmailRecipient.Create(sender.Name, sender.Email).Value;
}