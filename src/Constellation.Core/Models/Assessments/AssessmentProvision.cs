namespace Constellation.Core.Models.Assessments;

using Identifiers;
using ValueObjects;

public sealed class AssessmentProvision
{
    internal AssessmentProvision(
        Provision provision)
    {
        AssessmentStudentId = new();

        ProvisionId = provision.Id;
        Code = provision.Code;
        Description = provision.Description;
    }

    public AssessmentStudentId AssessmentStudentId { get; private set; }
    public ProvisionId ProvisionId { get; private set; }

    public ProvisionCode Code { get; private set; }
    public string Description { get; private set; }
}