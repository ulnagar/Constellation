namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Models.Identifiers;

public sealed class TutorialRollStudent
{
    public string StudentId { get; set; }
    public TutorialRollId TutorialRollId { get; set; }
    public bool Enrolled { get; set; }
    public bool Present { get; set; }
}