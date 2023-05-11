namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

internal class RoomCreation : ResponseDto
{
    [XmlElement("sco")]
    public RoomSco Sco { get; set; }
}
