namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

public class Status
{
    [XmlAttribute("code")]
    public string Code { get; set; }
    [XmlElement("invalid")]
    public StatusInvalid ErrorCode { get; set; }
}
