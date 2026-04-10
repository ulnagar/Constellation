namespace Constellation.Core.Models.Messaging.Email.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct EmailId(Guid Value)
    : IStronglyTypedId<EmailId, Guid>
{
    public static EmailId Empty => new(Guid.Empty);

    public static EmailId FromValue(Guid value) =>
        new(value);

    public EmailId()
        : this(Guid.CreateVersion7()) { }

    // Required by ASP.NET Core minimal API route binding
    public static bool TryParse(string? value, out EmailId result)
    {
        if (Guid.TryParse(value, out Guid guid))
        {
            result = new EmailId(guid);
            return true;
        }

        result = default!;
        return false;
    }

    public override string ToString() =>
        Value.ToString();
}