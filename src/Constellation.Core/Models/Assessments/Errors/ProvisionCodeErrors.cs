namespace Constellation.Core.Models.Assessments.Errors;

using Shared;

public static class ProvisionCodeErrors
{
    public static readonly Error EmptyValue = new(
        "Provision.Code.EmptyValue",
        "A Provision Code must have a value");

    public static readonly Error AlreadyExists = new(
        "Provision.Code.AlreadyExists",
        "A Provision with that Code has already been created");
}