namespace Constellation.Core.Models.Messaging.Drafts;

using ValueObjects;

public sealed class MessageRecipient : IEquatable<MessageRecipient>
{
    private MessageRecipient()
    {
        PhoneNumber = PhoneNumber.Empty;
        EmailAddress = EmailAddress.None;
        Name = string.Empty;
    }

    public MessageRecipient(
        PhoneNumber number,
        string name)
    {
        EmailAddress = EmailAddress.None;
        PhoneNumber = number;
        Name = name;
    }

    public MessageRecipient(
        EmailAddress email,
        string name)
    {
        EmailAddress = email;
        PhoneNumber = PhoneNumber.Empty;
        Name = name;
    }

    public MessageRecipient(
        EmailAddress email,
        PhoneNumber number,
        string name)
    {
        EmailAddress = email;
        PhoneNumber = number;
        Name = name;
    }

    public PhoneNumber PhoneNumber { get; init; }
    public EmailAddress EmailAddress { get; init; }
    public string Name { get; init; }
    
    public bool Equals(MessageRecipient? other) =>
        other is not null && 
        EmailAddress == other.EmailAddress && 
        PhoneNumber == other.PhoneNumber;

    public override bool Equals(object? obj) => 
        Equals(obj as MessageRecipient);

    public override int GetHashCode() => 
        HashCode.Combine(EmailAddress, PhoneNumber);

}