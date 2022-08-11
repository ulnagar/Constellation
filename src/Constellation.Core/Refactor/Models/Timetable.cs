namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System.Collections.Generic;

public class Timetable : BaseAuditableEntity
{
    public string Name { get; set; }
    public int DaysInCycle { get; set; }

    public IList<Period> Periods { get; private set; } = new List<Period>();
}
