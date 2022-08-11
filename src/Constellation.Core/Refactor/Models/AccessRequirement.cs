namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System;

public class AccessRequirement : BaseAuditableEntity
{
    public Guid PrincipalId { get; set; } //Who
    public string PrincipalType { get; set; } //nameof(Student), nameof(StaffMember) etc

    public Guid SystemResourceId { get; set; } //What
    public virtual SystemResource SystemResource { get; set; }
    
    public Guid AuthorisorId { get; set; } //Why
    public string AuthorisorType { get; set; } //nameof(Enrolment), nameof(ClassSession) etc
}
