namespace Constellation.Application.Domains.Training.Queries.GetCompletionRecordEditContext;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training.Identifiers;
using System;

public class CompletionRecordEditContextDto
{
    public TrainingCompletionId Id { get; set; }
    public StaffId StaffId { get; set; }
    public DateTime CompletedDate { get; set; }
    public TrainingModuleId TrainingModuleId { get; set; }
    public bool NotRequired { get; set; }
}
