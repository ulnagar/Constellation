using System;

namespace Constellation.Core.Models.MandatoryTraining;

public class TrainingCompletion
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public virtual Staff Staff { get; set; }
    public DateTime CompletedDate { get; set; }
    public Guid TrainingModuleId { get; set; }
    public virtual TrainingModule Module { get; set; }
}
