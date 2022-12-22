namespace Constellation.Core.Models.GroupTutorials;

using System;

public sealed class TutorialRollStudent
{
    public string StudentId { get; set; }
    public Guid TutorialRollId { get; set; }
    public bool Enrolled { get; set; }
    public bool Present { get; set; }
}