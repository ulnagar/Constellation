namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using Core.Models.Edval;

public sealed class LissPublishClassMemberships
{
    public string ClassCode { get; set; }
    public string EdvalClassCode { get; set; }
    public string StudentId { get; set; }

    public EdvalClassMembership ToClassMembership()
    {
        return new()
        {
            EdvalClassCode = EdvalClassCode,
            StudentId = StudentId,
            ClassCode = ClassCode
        };
    }
}