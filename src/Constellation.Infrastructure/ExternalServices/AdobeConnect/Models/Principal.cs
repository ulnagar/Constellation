namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

internal class Principal
{
    [XmlAttribute("principal-id")]
    public string PrincipalId { get; set; }
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("login")]
    public string Login { get; set; }
    [XmlElement("email")]
    public string Email { get; set; }
}