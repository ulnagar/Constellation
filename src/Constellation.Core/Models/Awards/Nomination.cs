namespace Constellation.Core.Models.Awards;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.ValueObjects;
using System;

public abstract class Nomination : IFullyAuditableEntity
{
    public AwardNominationId Id { get; protected set; }
    public AwardNominationPeriodId PeriodId { get; protected set; }
    public string StudentId { get; protected set; }
    public AwardType AwardType { get; protected set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; private set; }

    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    public abstract string GetDescription();
}

public sealed class FirstInSubjectNomination : Nomination
{
    private FirstInSubjectNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        int courseId, 
        string courseName)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.FirstInSubject;

        CourseId = courseId;
        CourseName = courseName;
    }

    public int CourseId { get; private set; }
    public string CourseName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName}";

    public override string GetDescription() => $"First in Course {CourseName}";
}

public sealed class AcademicExcellenceNomination : Nomination
{
    private AcademicExcellenceNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        int courseId,
        string courseName,
        int offeringId,
        string className)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.AcademicExcellence;

        CourseId = courseId;
        CourseName = courseName;
        OfferingId = offeringId;
        ClassName = className;
    }

    public int CourseId { get; private set; }
    public string CourseName { get; private set; }
    public int OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";

    public override string GetDescription() => $"Academic Excellence {CourseName}";
}

public sealed class AcademicAchievementNomination : Nomination
{
    private AcademicAchievementNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        int courseId,
        string courseName,
        int offeringId,
        string className)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.AcademicAchievement;

        CourseId = courseId;
        CourseName = courseName;
        OfferingId = offeringId;
        ClassName = className;
    }

    public int CourseId { get; private set; }
    public string CourseName { get; private set; }
    public int OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";
    public override string GetDescription() => $"Academic Achievement {CourseName} - {ClassName}";
}

public sealed class PrincipalsAwardNomination : Nomination
{
    private PrincipalsAwardNomination(
        AwardNominationPeriodId periodId,
        string studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.PrincipalsAward;
    }

    public override string ToString() => $"{AwardType.ToString()}";

    public override string GetDescription() => $"Principals Award";
}

public sealed class GalaxyMedalNomination : Nomination
{
    private GalaxyMedalNomination(
        AwardNominationPeriodId periodId,
        string studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.GalaxyMedal;
    }

    public override string ToString() => $"{AwardType.ToString()}";

    public override string GetDescription() => $"Galaxy Medal";
}

public sealed class UniversalAchieverNomination : Nomination
{
    private UniversalAchieverNomination(
        AwardNominationPeriodId periodId,
        string studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.PrincipalsAward;
    }

    public override string ToString() => $"{AwardType.ToString()}";
    public override string GetDescription() => $"Universal Achiever Award";
}