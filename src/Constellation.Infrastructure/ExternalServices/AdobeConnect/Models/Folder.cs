namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

public class Folder
{
    [XmlAttribute("sco-id")]
    public string ScoId { get; set; }
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("url-path")]
    public string UrlPath { get; set; }
}