namespace Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;

using Constellation.Core.Models.MandatoryTraining.Identifiers;
using System;

public class CompletionRecordEditContextDto
{
    public TrainingCompletionId Id { get; set; }
    public string StaffId { get; set; }
    public DateTime CompletedDate { get; set; }
    public TrainingModuleId TrainingModuleId { get; set; }
    public bool NotRequired { get; set; }
}
