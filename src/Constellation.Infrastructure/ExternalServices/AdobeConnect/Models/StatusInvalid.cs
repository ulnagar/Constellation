namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

internal class StatusInvalid
{
    [XmlAttribute("field")]
    public string Field { get; set; }
    [XmlAttribute("type")]
    public string Type { get; set; }
    [XmlAttribute("subcode")]
    public string Subcode { get; set; }
}
