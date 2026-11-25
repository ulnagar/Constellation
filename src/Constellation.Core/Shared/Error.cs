#nullable enable
namespace Constellation.Core.Shared;

using Microsoft.AspNetCore.Identity;
using System;

public class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }

    public static implicit operator string(Error error) => $"{error.Code}: {error.Message}";

    public static bool operator ==(Error? a, Error? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Error? a, Error? b) => !(a == b);

    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Message == other.Message;
    }

    public override bool Equals(object? obj) => obj is Error error && Equals(error);

    public override int GetHashCode() => HashCode.Combine(Code, Message);

    public override string ToString() => $"{Code}: {Message}";

    /// <summary>
    /// Converts a single <see cref="IdentityError"/> into an <see cref="Error"/>.
    /// The Identity error code is prefixed with "Identity." so it can't collide
    /// with domain error codes.
    /// </summary>
    public static Error FromIdentityError(IdentityError identityError)
    {
        ArgumentNullException.ThrowIfNull(identityError);

        var code = string.IsNullOrWhiteSpace(identityError.Code)
            ? "Identity.Unknown"
            : $"Identity.{identityError.Code}";

        var message = string.IsNullOrWhiteSpace(identityError.Description)
            ? "An identity error occurred."
            : identityError.Description;

        return new Error(code, message);
    }

    /// <summary>
    /// Converts a failed <see cref="IdentityResult"/> into a single <see cref="Error"/>,
    /// so it can be passed to <c>Result.Failure(Error)</c>.
    /// </summary>
    /// <remarks>
    /// One error maps directly. Several errors are combined into an
    /// "Identity.Multiple" error with the messages joined, because the Result
    /// type carries a single error. If a failed result has no errors, an
    /// "Identity.Unknown" error is returned.
    /// </remarks>
    public static Error FromIdentityResult(IdentityResult identityResult)
    {
        ArgumentNullException.ThrowIfNull(identityResult);

        var errors = identityResult.Errors.ToList();

        return errors.Count switch
        {
            0 => new Error("Identity.Unknown", "An identity operation failed."),
            1 => FromIdentityError(errors[0]),
            _ => new Error(
                "Identity.Multiple",
                string.Join(" ", errors.Select(e => e.Description)))
        };
    }
}