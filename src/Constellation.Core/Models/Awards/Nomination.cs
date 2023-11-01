namespace Constellation.Core.Models.Awards;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.ValueObjects;
using Enums;
using Extensions;
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
    public FirstInSubjectNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId, 
        Grade grade,
        string courseName)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.FirstInSubject;

        CourseId = courseId;
        Grade = grade;
        CourseName = courseName;
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public Grade Grade { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;

    public override string ToString() => $"{AwardType.ToString()} in {Grade.AsName()} {CourseName}";

    public override string GetDescription() => $"First in Course {Grade.AsName()} {CourseName}";
}

public sealed class AcademicExcellenceNomination : Nomination
{
    public AcademicExcellenceNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId,
        string courseName,
        OfferingId offeringId,
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

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";

    public override string GetDescription() => $"Academic Excellence {CourseName} - {ClassName}";
}

public sealed class AcademicExcellenceMathematicsNomination : Nomination
{
    public AcademicExcellenceMathematicsNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId,
        string courseName,
        OfferingId offeringId,
        string className)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.AcademicExcellenceMathematics;

        CourseId = courseId;
        CourseName = courseName;
        OfferingId = offeringId;
        ClassName = className;
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";

    public override string GetDescription() => $"Academic Excellence - Mathematics - {CourseName} - {ClassName}";
}

public sealed class AcademicExcellenceScienceTechnologyNomination : Nomination
{
    public AcademicExcellenceScienceTechnologyNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId,
        string courseName,
        OfferingId offeringId,
        string className)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.AcademicExcellenceScienceTechnology;

        CourseId = courseId;
        CourseName = courseName;
        OfferingId = offeringId;
        ClassName = className;
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";

    public override string GetDescription() => $"Academic Excellence - Science & Technology - {CourseName} - {ClassName}";
}

public sealed class AcademicAchievementNomination : Nomination
{
    public AcademicAchievementNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId,
        string courseName,
        OfferingId offeringId,
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

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";
    public override string GetDescription() => $"Academic Achievement {CourseName} - {ClassName}";
}

public sealed class AcademicAchievementMathematicsNomination : Nomination
{
    public AcademicAchievementMathematicsNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId,
        string courseName,
        OfferingId offeringId,
        string className)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.AcademicAchievementMathematics;

        CourseId = courseId;
        CourseName = courseName;
        OfferingId = offeringId;
        ClassName = className;
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";
    public override string GetDescription() => $"Academic Achievement - Mathematics - {CourseName} - {ClassName}";
}

public sealed class AcademicAchievementScienceTechnologyNomination : Nomination
{
    public AcademicAchievementScienceTechnologyNomination(
        AwardNominationPeriodId periodId,
        string studentId,
        CourseId courseId,
        string courseName,
        OfferingId offeringId,
        string className)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.AcademicAchievementScienceTechnology;

        CourseId = courseId;
        CourseName = courseName;
        OfferingId = offeringId;
        ClassName = className;
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public string ClassName { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;
    public void UpdateClassName(string className) => ClassName = className;

    public override string ToString() => $"{AwardType.ToString()} in {CourseName} for {ClassName}";
    public override string GetDescription() => $"Academic Achievement - Science & Technology - {CourseName} - {ClassName}";
}

public sealed class PrincipalsAwardNomination : Nomination
{
    public PrincipalsAwardNomination(
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
    public GalaxyMedalNomination(
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
    public UniversalAchieverNomination(
        AwardNominationPeriodId periodId,
        string studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.UniversalAchiever;
    }

    public override string ToString() => $"{AwardType.ToString()}";
    public override string GetDescription() => $"Universal Achiever Award";
}