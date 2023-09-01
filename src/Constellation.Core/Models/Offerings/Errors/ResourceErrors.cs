namespace Constellation.Core.Models.Offerings.Errors;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using System;

public static class ResourceErrors
{
    public static readonly Func<ResourceId, Error> NotFound = id => new(
        "Offerings.Resource.NotFound",
        $"Could not find a Resource with the Id {id}");

    public static readonly Func<string, Error> InvalidType = type => new(
        "Offerings.Resourece.InvalidType",
        $"The resource type {type} is not a valid option");
}