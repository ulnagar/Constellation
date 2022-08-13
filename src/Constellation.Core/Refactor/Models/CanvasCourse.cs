namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;

public class CanvasCourse : BaseAuditableEntity
{
    public int CanvasId { get; set; }
    public string Name { get; set; }
}
