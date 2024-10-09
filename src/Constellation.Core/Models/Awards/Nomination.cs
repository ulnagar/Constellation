namespace Constellation.Core.Models.Awards;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Enums;
using Extensions;
using Identifiers;
using Primitives;
using System;
using ValueObjects;

public abstract class Nomination : IFullyAuditableEntity
{
    public AwardNominationId Id { get; protected set; }
    public AwardNominationPeriodId PeriodId { get; protected set; }
    public StudentId StudentId { get; protected set; }
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

    public abstract string GetDescription(bool showGrade = true, bool showClass = true);
}

public sealed class FirstInSubjectNomination : Nomination
{
    public FirstInSubjectNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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

    public override string GetDescription(bool showGrade = true, bool showClass = true) =>
        (showClass && showGrade)
            ? $"First in Course {Grade.AsName()} {CourseName}"
        : showClass
            ? $"First in Course {CourseName}"
            : $"First in Course {Grade.AsName()}";
}

public sealed class AcademicExcellenceNomination : Nomination
{
    public AcademicExcellenceNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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

    public override string GetDescription(bool showGrade = true, bool showClass = true) =>
        showClass
            ? $"Academic Excellence {CourseName} - {ClassName}"
            : $"Academic Excellence {CourseName}";
}

public sealed class AcademicExcellenceMathematicsNomination : Nomination
{
    public AcademicExcellenceMathematicsNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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

    public override string GetDescription(bool showGrade = true, bool showClass = true) =>
        showClass
            ? $"Academic Excellence - Mathematics - {CourseName} - {ClassName}"
            : $"Academic Excellence - Mathematics - {CourseName}";
}

public sealed class AcademicExcellenceScienceTechnologyNomination : Nomination
{
    public AcademicExcellenceScienceTechnologyNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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

    public override string GetDescription(bool showGrade = true, bool showClass = true) => 
        showClass 
            ? $"Academic Excellence - Science & Technology - {CourseName} - {ClassName}"
            : $"Academic Excellence - Science & Technology - {CourseName}";
}

public sealed class AcademicAchievementNomination : Nomination
{
    public AcademicAchievementNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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
    public override string GetDescription(bool showGrade = true, bool showClass = true) => 
        showClass 
            ? $"Academic Achievement {CourseName} - {ClassName}" 
            : $"Academic Achievement {CourseName}";
}

public sealed class AcademicAchievementMathematicsNomination : Nomination
{
    public AcademicAchievementMathematicsNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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
    public override string GetDescription(bool showGrade = true, bool showClass = true) => 
        showClass 
            ? $"Academic Achievement - Mathematics - {CourseName} - {ClassName}"
            : $"Academic Achievement - Mathematics - {CourseName}";
}

public sealed class AcademicAchievementScienceTechnologyNomination : Nomination
{
    public AcademicAchievementScienceTechnologyNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
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
    public override string GetDescription(bool showGrade = true, bool showClass = true) => 
        showClass 
            ? $"Academic Achievement - Science & Technology - {CourseName} - {ClassName}"
            : $"Academic Achievement - Science & Technology - {CourseName}";
}

public sealed class PrincipalsAwardNomination : Nomination
{
    public PrincipalsAwardNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.PrincipalsAward;
    }

    public override string ToString() => $"{AwardType.ToString()}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) => $"Principals Award";
}

public sealed class GalaxyMedalNomination : Nomination
{
    public GalaxyMedalNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.GalaxyMedal;
    }

    public override string ToString() => $"{AwardType.ToString()}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) => $"Galaxy Medal";
}

public sealed class UniversalAchieverNomination : Nomination
{
    public UniversalAchieverNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.UniversalAchiever;
    }

    public override string ToString() => $"{AwardType.ToString()}";
    public override string GetDescription(bool showGrade = true, bool showClass = true) => $"Universal Achiever Award";
}