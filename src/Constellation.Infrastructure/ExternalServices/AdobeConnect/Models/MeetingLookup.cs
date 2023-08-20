namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

public class MeetingLookup : ResponseDto
{
    [XmlArray("expanded-scos"), XmlArrayItem("sco")]
    public List<MeetingRoom> Rooms { get; set; }
}
