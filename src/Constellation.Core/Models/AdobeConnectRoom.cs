using System;

namespace Constellation.Core.Models;

public class AdobeConnectRoom
{
    public string ScoId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UrlPath { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public bool Protected { get; set; }
}