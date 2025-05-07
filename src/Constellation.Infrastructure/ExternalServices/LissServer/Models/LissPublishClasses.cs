namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using Core.Models.Edval;

public sealed class LissPublishClasses
{
    public string ClassCode { get; set; }
    public string EdvalClassCode { get; set; }
    public string Form { get; set; }

    public EdvalClass ToClass()
    {
        return new()
        {
            ClassCode = ClassCode,
            EdvalClassCode = EdvalClassCode,
            Form = Form
        };
    }
}