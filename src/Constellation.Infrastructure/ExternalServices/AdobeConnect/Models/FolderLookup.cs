namespace Constellation.Infrastructure.ExternalServices.AdobeConnect.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

internal class FolderLookup : ResponseDto
{
    [XmlArray("scos"), XmlArrayItem("sco")]
    public List<Folder> Scos { get; set; }
}
