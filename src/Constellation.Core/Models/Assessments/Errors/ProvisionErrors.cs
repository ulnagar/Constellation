namespace Constellation.Core.Models.Assessments.Errors;

using Identifiers;
using Shared;

public static class ProvisionErrors
{
    public static readonly Func<ProvisionId, Error> NotFound = id => new(
        "Provision.NotFound",
        $"A Provision with the Id {id} could not be found");
}