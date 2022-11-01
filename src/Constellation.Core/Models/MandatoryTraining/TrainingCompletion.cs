namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Common;
using System;

public class TrainingCompletion : AuditableEntity
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public virtual Staff Staff { get; set; }
    public DateTime CompletedDate { get; set; }
    public Guid TrainingModuleId { get; set; }
    public virtual TrainingModule Module { get; set; }
}
