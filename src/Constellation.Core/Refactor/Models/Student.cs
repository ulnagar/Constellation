namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;
using System.Collections.Generic;

public class Student : BaseAuditableEntity
{
    public string SRN { get; set; }
    public string FirstName { get; set; }
    public string PreferredName { get; set; }
    public string LastName { get; set; }
    public string DisplayName => $"{(string.IsNullOrWhiteSpace(PreferredName) ? FirstName : PreferredName)} {LastName}";
    public Gender Gender { get; set; }

    public string PortalUsername { get; set; }
    public string EmailAddress => PortalUsername + "@education.nsw.gov.au";

    public IList<Grade> Cohorts { get; private set; } = new List<Grade>();

    public Guid SchoolId { get; set; }
    public virtual School School { get; set; }

    public Guid PhotoId { get; set; }
    public virtual StudentPhoto Photo { get; set; }

    public Guid FamilyId { get; set; }
    public virtual Family Family { get; set; }

    public IList<Enrolment> Enrolments { get; private set; } = new List<Enrolment>();
}