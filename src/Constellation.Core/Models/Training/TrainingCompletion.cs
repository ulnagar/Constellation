namespace Constellation.Core.Models.Training;

using Identifiers;
using Primitives;
using System;

public sealed class TrainingCompletion : IAuditableEntity
{
    public TrainingCompletion(
        string staffId,
        TrainingModuleId trainingModuleId,
        DateOnly completedDate)
    {
        Id = new();
        StaffId = staffId;
        TrainingModuleId = trainingModuleId;
        CompletedDate = completedDate;
    }

    public TrainingCompletionId Id { get; private set; }
    public string StaffId { get; private set; }
    public DateOnly CompletedDate { get; private set; }
    public TrainingModuleId TrainingModuleId { get; private set; }
    public TrainingModule Module { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static TrainingCompletion Create(
        string staffId,
        TrainingModuleId moduleId,
        DateOnly completedDate)
    {
        return new(
            staffId,
            moduleId,
            completedDate);
    }
    
    public void Delete() => IsDeleted = true;
}
