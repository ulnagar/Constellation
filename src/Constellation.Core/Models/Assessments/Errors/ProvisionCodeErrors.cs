namespace Constellation.Core.Models.Assessments.Errors;

using Shared;

public static class ProvisionCodeErrors
{
    public static readonly Error EmptyValue = new(
        "ProvisionCode.EmptyValue",
        "A Provision Code must have a value");
}