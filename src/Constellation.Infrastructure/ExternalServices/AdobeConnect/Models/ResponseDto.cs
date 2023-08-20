namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

[XmlRoot("results")]
public class ResponseDto
{
    [XmlElement("status")]
    public Status Status { get; set; }
}
