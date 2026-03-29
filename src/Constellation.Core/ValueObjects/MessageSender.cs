namespace Constellation.Core.ValueObjects;

using Primitives;

public sealed class MessageSender : ValueObject
{
    private MessageSender() { }

    private MessageSender(string name, string destination)
    {
        Name = name;
        Destination = destination;
    }
    
    public string Name { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Destination;
    }

    // Implicit conversion from EmailRecipient — replaces the From() factory method
    public static implicit operator MessageSender(EmailRecipient recipient) =>
        new(recipient.Name, recipient.Email);

    // Implicit conversion back to EmailRecipient — useful if you need to pass
    // an EmailSender to something expecting an EmailRecipient
    public static implicit operator EmailRecipient(MessageSender sender) =>
        EmailRecipient.Create(sender.Name, sender.Destination).Value;

    public static implicit operator MessageSender(SmsRecipient recipient) =>
        new(recipient.Name, recipient.Number);

    public static implicit operator SmsRecipient(MessageSender sender) =>
        SmsRecipient.Create(sender.Name, sender.Destination).Value;
}
