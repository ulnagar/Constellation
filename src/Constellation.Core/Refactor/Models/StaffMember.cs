namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;
using System.Collections.Generic;

public class StaffMember : BaseAuditableEntity
{
    public string EmployeeId { get; set; }
    public string FirstName { get; set; }
    public string PreferredName { get; set; }
    public string LastName { get; set; }
    public string DisplayName => $"{(string.IsNullOrWhiteSpace(PreferredName) ? FirstName : PreferredName)} {LastName}";
    public Gender Gender { get; set; }

    public string PortalUsername { get; set; }
    public string EmailAddress => PortalUsername + "@det.nsw.edu.au";

    public Guid PhotoId { get; set; }
    public virtual StaffPhoto Photo { get; set; }

    public IList<Faculty> Faculties { get; private set; } = new List<Faculty>();

    public Guid SchoolId { get; set; }
    public virtual School School { get; set; }

    public IList<ClassSession> Sessions { get; private set; } = new List<ClassSession>();
}
