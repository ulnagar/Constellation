namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;

public class TrainingCompletion : IAuditableEntity
{
    public TrainingCompletion(
        string staffId,
        TrainingModuleId trainingModuleId)
    {
        Id = new();
        StaffId = staffId;
        TrainingModuleId = trainingModuleId;
    }

    public TrainingCompletionId Id { get; private set; }
    public string StaffId { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public bool NotRequired { get; private set; }
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
        TrainingModuleId moduleId)
    {
        return new(
            staffId,
            moduleId);
    }

    public void UpdateStaffMember(string staffId) => StaffId = staffId;

    public void UpdateTrainingModule(TrainingModuleId moduleId) => TrainingModuleId = moduleId;

    public void Delete() => IsDeleted = true;

    public void MarkNotRequired(TrainingModule module)
    {
        // Check that this is valid on the Module
        bool canMarkNotRequired = module.CanMarkNotRequired;
        bool sameModule = module.Id == TrainingModuleId;

        if (canMarkNotRequired && sameModule)
        {
            NotRequired = true;
            CompletedDate = null;
        }
    }

    public void SetCompletedDate(DateTime completedDate)
    {
        if (NotRequired)
        {
            NotRequired = false;
        }

        CompletedDate = completedDate;
    }
}
