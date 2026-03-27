namespace Constellation.Core.Models.Messaging.Drafts;

using Identifiers;
using ValueObjects;

public sealed class MessageRecipient : IEquatable<MessageRecipient>
{
    private MessageRecipient()
    {
        Id = new();
        PhoneNumber = PhoneNumber.Empty;
        EmailAddress = EmailAddress.None;
        Name = string.Empty;
    }

    public MessageRecipient(
        PhoneNumber number,
        string name)
    {
        Id = new();
        EmailAddress = EmailAddress.None;
        PhoneNumber = number;
        Name = name;
    }

    public MessageRecipient(
        EmailAddress email,
        string name)
    {
        Id = new();
        EmailAddress = email;
        PhoneNumber = PhoneNumber.Empty;
        Name = name;
    }

    public MessageRecipient(
        EmailAddress email,
        PhoneNumber number,
        string name)
    {
        Id = new();
        EmailAddress = email;
        PhoneNumber = number;
        Name = name;
    }

    public MessageRecipientId Id { get; init; }
    public PhoneNumber PhoneNumber { get; init; }
    public EmailAddress EmailAddress { get; init; }
    public string Name { get; init; }

    public bool HasEmail => EmailAddress != EmailAddress.None;
    public bool HasPhone => PhoneNumber != PhoneNumber.Empty;

    public bool Equals(MessageRecipient? other) =>
        other is not null && 
        EmailAddress == other.EmailAddress && 
        PhoneNumber == other.PhoneNumber;

    public override bool Equals(object? obj) => 
        Equals(obj as MessageRecipient);

    public override int GetHashCode() => 
        HashCode.Combine(EmailAddress, PhoneNumber);

}