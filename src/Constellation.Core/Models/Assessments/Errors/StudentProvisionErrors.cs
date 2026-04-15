namespace Constellation.Core.Models.Assessments.Errors;

using Identifiers;
using Shared;

public static class StudentProvisionErrors
{
    public static readonly Error AlreadyExists = new(
        "StudentProvision.AlreadyExists",
        "An active Student Provision already exists");

    public static readonly Func<StudentProvisionId, Error> NotFound = id => new(
        "StudentProvision.NotFound",
        $"A Student Provision with the Id {id} could not be found");
}