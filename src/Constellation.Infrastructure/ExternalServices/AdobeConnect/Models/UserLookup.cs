namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

public class UserLookup : ResponseDto
{
    [XmlArray("principal-list"), XmlArrayItem("principal")]
    public List<Principal> PrincipalList { get; set; }
}

