using Constellation.Core.Refactor.Common;
using System;

namespace Constellation.Core.Refactor.Models;

public class Enrolment : BaseAuditableEntity
{
    public Guid StudentId { get; set; }
    public virtual Student Student { get; set; }

    public Guid ClassId { get; set; }
    public virtual Class Class { get; set; }
}
