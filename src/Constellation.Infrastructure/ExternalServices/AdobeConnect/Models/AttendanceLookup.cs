namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

internal class AttendanceLookup : ResponseDto
{
    [XmlArray("report-meeting-attendance"), XmlArrayItem("row")]
    public List<UserAttendance> Users { get; set; }
}

