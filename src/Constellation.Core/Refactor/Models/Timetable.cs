namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;

public class Timetable : BaseAuditableEntity
{
    public string Name { get; set; }
    public int DaysInCycle { get; set; }
}
