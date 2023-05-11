namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

[XmlRoot("results")]
internal class ResponseDto
{
    [XmlElement("status")]
    public Status Status { get; set; }
}
