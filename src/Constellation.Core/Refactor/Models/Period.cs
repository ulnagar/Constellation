namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;
using System.Collections.Generic;

public class Period : BaseAuditableEntity
{
    public Guid TimetableId { get; set; }
    public virtual Timetable Timetable { get; set; }

    public int DayOfCycle { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string Name { get; set; }
    public PeriodType PeriodType { get; set; }

    public IList<ClassSession> Sessions { get; private set; } = new List<ClassSession>();
}
