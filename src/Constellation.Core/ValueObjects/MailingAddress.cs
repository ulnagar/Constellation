namespace Constellation.Core.ValueObjects;

using Errors;
using Primitives;
using Shared;

public sealed class MailingAddress : ValueObject
{
    private static readonly IReadOnlySet<string> _validStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "NSW", "VIC", "QLD", "SA", "WA", "TAS", "ACT", "NT"
    };

    private MailingAddress() { } // Required by EF Core

    private MailingAddress(string street, string town, string state, string postcode)
    {
        Street = street;
        Town = town;
        State = state;
        Postcode = postcode;
    }

    public static Result<MailingAddress> Create(string street, string town, string state, string postcode)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<MailingAddress>(MailingAddressErrors.StreetEmpty);

        if (string.IsNullOrWhiteSpace(town))
            return Result.Failure<MailingAddress>(MailingAddressErrors.TownEmpty);

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure<MailingAddress>(MailingAddressErrors.StateEmpty);

        if (!_validStates.Contains(state))
            return Result.Failure<MailingAddress>(MailingAddressErrors.StateInvalid);

        if (string.IsNullOrWhiteSpace(postcode))
            return Result.Failure<MailingAddress>(MailingAddressErrors.PostcodeEmpty);

        if (postcode.Length != 4 || !postcode.All(char.IsAsciiDigit))
            return Result.Failure<MailingAddress>(MailingAddressErrors.PostcodeInvalid);

        return new MailingAddress(
            street.Trim(),
            town.Trim(),
            state.Trim().ToUpperInvariant(),
            postcode.Trim());
    }

    public string Street { get; private set; }
    public string Town { get; private set; }
    public string State { get; private set; }
    public string Postcode { get; private set; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Street;
        yield return Town;
        yield return State;
        yield return Postcode;
    }

    public override string ToString() => $"{Street}, {Town} {State} {Postcode}";
}