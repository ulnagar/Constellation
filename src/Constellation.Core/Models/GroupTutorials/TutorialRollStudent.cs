namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;

public sealed class TutorialRollStudent : Entity, IAuditableEntity
{
    public TutorialRollStudent(Guid Id)
        : base(Id)
    {
    }

    public string StudentId { get; set; }
    public Student Student { get; set; }
    public Guid TutorialRollId { get; set; }
    public TutorialRoll TutorialRoll { get; set; }
    public bool Present { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}