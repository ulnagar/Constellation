namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;

public sealed class TutorialRollStudent
{
    public StudentId StudentId { get; set; }
    public TutorialRollId TutorialRollId { get; set; }
    public bool Enrolled { get; set; }
    public bool Present { get; set; }
}