namespace Constellation.Core.Models.Assessments;

using Identifiers;
using ValueObjects;

public sealed class Provision
{
    public Provision(
        ProvisionCode code,
        string description)
    {
        Id = new();

        Code = code;
        Description = description;
    }

    public ProvisionId Id { get; init; }
    public ProvisionCode Code { get; private set; }
    public string Description { get; private set; }
}