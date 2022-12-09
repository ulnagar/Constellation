namespace Constellation.Core.Models.MandatoryTraining;

using Constellation.Core.Primitives;
using System;

public class TrainingCompletion : AuditableEntity
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public virtual Staff Staff { get; set; }
    public DateTime? CompletedDate { get; private set; }
    public bool NotRequired { get; private set; }
    public Guid TrainingModuleId { get; set; }
    public virtual TrainingModule Module { get; set; }
    public int? StoredFileId { get; set; }
    public virtual StoredFile StoredFile { get; set; }

    public void MarkNotRequired()
    {
        // Check that this is valid on the Module
        var canMarkNotRequired = Module.CanMarkNotRequired;

        if (canMarkNotRequired)
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
