namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;

public sealed class TutorialRollStudent : IAuditableEntity
{
    public string StudentId { get; set; }
    public Guid TutorialRollId { get; set; }
    public bool Present { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}