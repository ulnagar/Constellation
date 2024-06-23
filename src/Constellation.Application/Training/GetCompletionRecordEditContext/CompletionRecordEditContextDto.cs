namespace Constellation.Application.Training.GetCompletionRecordEditContext;

using Core.Models.Training.Identifiers;
using System;

public class CompletionRecordEditContextDto
{
    public TrainingCompletionId Id { get; set; }
    public string StaffId { get; set; }
    public DateTime CompletedDate { get; set; }
    public TrainingModuleId TrainingModuleId { get; set; }
    public bool NotRequired { get; set; }
}
