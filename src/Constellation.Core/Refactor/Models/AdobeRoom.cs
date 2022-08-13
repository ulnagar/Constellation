using Constellation.Core.Refactor.Common;

namespace Constellation.Core.Refactor.Models;

public class AdobeRoom : BaseAuditableEntity
{
    public string ScoId { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
}
