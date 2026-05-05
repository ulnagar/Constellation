namespace Constellation.Core.Models.Hosting;

using Errors;
using Shared;
using System.Runtime.CompilerServices;

public sealed class Livestream
{
    private Livestream(
        string name,
        string embedCode,
        string description,
        DateOnly startsOn,
        DateOnly expiresOn)
    {
        Id = Guid.NewGuid();

        Name = name;
        EmbedCode = embedCode;
        Description = description;
        StartsOn = startsOn;
        ExpiresOn = expiresOn;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string EmbedCode { get; private set; }
    public string Description { get; private set; }

    public DateOnly StartsOn { get; private set; }
    public DateOnly ExpiresOn { get; private set; }

    public bool IsActive => IsCurrentOrFuture();

    private bool IsCurrentOrFuture()
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

        if (StartsOn >= currentDate)
            return true;

        if (ExpiresOn >= currentDate)
            return true;

        return false;
    }

    public static Result<Livestream> Create(
        string name,
        string embedCode,
        string? description,
        DateOnly startsOn,
        DateOnly expiresOn)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Livestream>(LivestreamErrors.MustIncludeName);

        if (string.IsNullOrWhiteSpace(embedCode))
            return Result.Failure<Livestream>(LivestreamErrors.MustIncludeEmbedCode);

        description = !string.IsNullOrWhiteSpace(description)
            ? description
            : string.Empty;

        if (expiresOn < startsOn)
            return Result.Failure<Livestream>(LivestreamErrors.InvalidExpiryDate);

        if (expiresOn < DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<Livestream>(LivestreamErrors.ExpiryDateMustBeInTheFuture);

        return new Livestream(
            name,
            embedCode, 
            description, 
            startsOn, 
            expiresOn);
    }

    public Result Update(
        string name,
        string embedCode,
        string? description,
        DateOnly startsOn,
        DateOnly expiresOn)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Livestream>(LivestreamErrors.MustIncludeName);

        if (string.IsNullOrWhiteSpace(embedCode))
            return Result.Failure<Livestream>(LivestreamErrors.MustIncludeEmbedCode);

        description = !string.IsNullOrWhiteSpace(description)
            ? description
            : string.Empty;

        if (expiresOn < startsOn)
            return Result.Failure<Livestream>(LivestreamErrors.InvalidExpiryDate);

        if (expiresOn < DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<Livestream>(LivestreamErrors.ExpiryDateMustBeInTheFuture);

        Name = name;
        EmbedCode = embedCode;
        Description = description;
        StartsOn = startsOn;
        ExpiresOn = expiresOn;

        return Result.Success();
    }
}