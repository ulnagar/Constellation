namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System.Collections.Generic;

public class Grade : BaseAuditableEntity
{
    public string Name { get; set; }
    public int CohortYear { get; set; }
    public string Description => $"{CohortYear} {Name}";

    public IList<Student> Students { get; private set; } = new List<Student>();

    // public IList<StaffMember> YearAdvisor { get; set; }
}