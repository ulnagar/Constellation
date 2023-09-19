using System;

namespace Constellation.Core.Models;

public class AdobeConnectRoom
{
    public string ScoId { get; set; }
    public string Name { get; set; }
    public string UrlPath { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public bool Protected { get; set; }
}