﻿namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

public class SessionLookup : ResponseDto
{
    [XmlArray("report-meeting-sessions"), XmlArrayItem("row")]
    public List<MeetingSession> Sessions { get; set; }
}
