namespace Constellation.Core.Models.Offerings.Errors;

using Identifiers;
using Shared;
using System;
using ValueObjects;

public static class ResourceErrors
{
    public static readonly Func<ResourceId, Error> NotFound = id => new(
        "Offerings.Resource.NotFound",
        $"Could not find a Resource with the Id {id}");

    public static readonly Func<ResourceType, Error> NoneOfTypeFound = type => new(
        "Offerings.Resource.NoneOfTypeFound",
        $"Could not find a Resource of Type {type} associated with the Offering");

    public static readonly Func<string, Error> InvalidType = type => new(
        "Offerings.Resourece.InvalidType",
        $"The resource type {type} is not a valid option");
}