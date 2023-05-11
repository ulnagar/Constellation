namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Xml.Serialization;

internal class MeetingSession
{
    [XmlAttribute("sco-id")]
    public string ScoId { get; set; }
    [XmlAttribute("asset-id")]
    public string AssetId { get; set; }
    [XmlAttribute("num-participants")]
    public string ParticipantCount { get; set; }
    [XmlElement("date-created")]
    public DateTime? DateStarted { get; set; }
    [XmlElement("date-end")]
    public DateTime? DateEnded { get; set; }
}