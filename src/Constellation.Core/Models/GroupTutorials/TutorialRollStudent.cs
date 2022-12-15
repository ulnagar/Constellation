namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;

public sealed class TutorialRollStudent : IAuditableEntity
{
    public string StudentId { get; set; } = string.Empty;
    public Guid TutorialRollId { get; set; }
    public bool Present { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}