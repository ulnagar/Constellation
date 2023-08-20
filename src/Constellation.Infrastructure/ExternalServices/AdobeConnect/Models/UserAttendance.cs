namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

public class UserAttendance
{
    [XmlAttribute("principal-id")]
    public string PrincipalId { get; set; }
    [XmlElement("login")]
    public string Login { get; set; }
    [XmlElement("session-name")]
    public string Name { get; set; }
    [XmlElement("date-created")]
    public DateTime? DateEntered { get; set; }
    [XmlElement("date-end")]
    public DateTime? DateLeft { get; set; }
}
