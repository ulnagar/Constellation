namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Common;
using System;

public sealed class TutorialRollStudent : AuditableEntity
{
    public Guid Id { get; set; }
    public string StudentId { get; set; }
    public Student Student { get; set; }
    public string StudentGrade { get; set; }
    public string StudentSchoolCode { get; set; }
    public string StudentSchoolName { get; set; }
    public Guid TutorialRollId { get; set; }
    public TutorialRoll TutorialRoll { get; set; }
    public bool Present { get; set; }
}