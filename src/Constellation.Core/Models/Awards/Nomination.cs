namespace Constellation.Core.Models.Awards;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Enums;
using Errors;
using Extensions;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Linq;
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
    private FirstInSubjectNomination(
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

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        Grade grade,
        string courseName)
    {
        if (AwardType.FirstInSubject.Grades.Count > 0 && !AwardType.FirstInSubject.Grades.Contains(grade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.FirstInSubject, grade));

        return new FirstInSubjectNomination(
            periodId,
            studentId,
            courseId,
            grade,
            courseName);
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public Grade Grade { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;

    public override string ToString() => $"{AwardType.ToString()} in {Grade.AsName()} {CourseName}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) =>
        (showClass && showGrade)
            ? $"First in Course {Grade.AsName()} {CourseName}"
        : (showClass)
            ? $"First in Course {CourseName}"
        : (showGrade)
            ? $"First in Course {Grade.AsName()}"
            : $"First in Course";
}

public sealed class FirstInSubjectMathematicsNomination : Nomination
{
    private FirstInSubjectMathematicsNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        Grade grade,
        string courseName)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.FirstInSubjectMathematics;

        CourseId = courseId;
        Grade = grade;
        CourseName = courseName;
    }

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        Grade grade,
        string courseName)
    {
        if (AwardType.FirstInSubjectMathematics.Grades.Count > 0 && !AwardType.FirstInSubjectMathematics.Grades.Contains(grade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.FirstInSubjectMathematics, grade));

        return new FirstInSubjectMathematicsNomination(
            periodId,
            studentId,
            courseId,
            grade,
            courseName);
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public Grade Grade { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;

    public override string ToString() => $"{AwardType.ToString()} in {Grade.AsName()} {CourseName}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) =>
        (showClass && showGrade)
            ? $"First in Course - Mathematics - {Grade.AsName()} {CourseName}"
            : (showClass)
                ? $"First in Course - Mathematics - {CourseName}"
                : (showGrade)
                    ? $"First in Course - Mathematics - {Grade.AsName()}"
                    : $"First in Course - Mathematics";
}

public sealed class FirstInSubjectScienceTechnologyNomination : Nomination
{
    private FirstInSubjectScienceTechnologyNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        Grade grade,
        string courseName)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.FirstInSubjectScienceTechnology;

        CourseId = courseId;
        Grade = grade;
        CourseName = courseName;
    }

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        Grade grade,
        string courseName)
    {
        if (AwardType.FirstInSubjectScienceTechnology.Grades.Count > 0 && !AwardType.FirstInSubjectScienceTechnology.Grades.Contains(grade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.FirstInSubjectScienceTechnology, grade));

        return new FirstInSubjectScienceTechnologyNomination(
            periodId,
            studentId,
            courseId,
            grade,
            courseName);
    }

    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public Grade Grade { get; private set; }

    public void UpdateCourseName(string courseName) => CourseName = courseName;

    public override string ToString() => $"{AwardType.ToString()} in {Grade.AsName()} {CourseName}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) =>
        (showClass && showGrade)
            ? $"First in Course - Science and Technology - {Grade.AsName()} {CourseName}"
            : (showClass)
                ? $"First in Course - Science and Technology - {CourseName}"
                : (showGrade)
                    ? $"First in Course - Science and Technology - {Grade.AsName()}"
                    : $"First in Course - Science and Technology";
}

public sealed class AcademicExcellenceNomination : Nomination
{
    private AcademicExcellenceNomination(
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

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        string courseName,
        Grade courseGrade,
        OfferingId offeringId,
        string className)
    {
        if (AwardType.AcademicExcellence.Grades.Count > 0 && !AwardType.AcademicExcellence.Grades.Contains(courseGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.AcademicExcellence, courseGrade));

        return new AcademicExcellenceNomination(
            periodId,
            studentId,
            courseId,
            courseName,
            offeringId,
            className);
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
    private AcademicExcellenceMathematicsNomination(
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

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        string courseName,
        Grade courseGrade,
        OfferingId offeringId,
        string className)
    {
        if (AwardType.AcademicExcellenceMathematics.Grades.Count > 0 && !AwardType.AcademicExcellenceMathematics.Grades.Contains(courseGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.AcademicExcellenceMathematics, courseGrade));

        return new AcademicExcellenceMathematicsNomination(
            periodId,
            studentId,
            courseId,
            courseName,
            offeringId,
            className);
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
    private AcademicExcellenceScienceTechnologyNomination(
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

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        string courseName,
        Grade courseGrade,
        OfferingId offeringId,
        string className)
    {
        if (AwardType.AcademicExcellenceScienceTechnology.Grades.Count > 0 && !AwardType.AcademicExcellenceScienceTechnology.Grades.Contains(courseGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.AcademicExcellenceScienceTechnology, courseGrade));

        return new AcademicExcellenceScienceTechnologyNomination(
            periodId,
            studentId,
            courseId,
            courseName,
            offeringId,
            className);
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
    private AcademicAchievementNomination(
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
    
    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        string courseName,
        Grade courseGrade,
        OfferingId offeringId,
        string className)
    {
        if (AwardType.AcademicAchievement.Grades.Count > 0 && !AwardType.AcademicAchievement.Grades.Contains(courseGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.AcademicAchievement, courseGrade));

        return new AcademicAchievementNomination(
            periodId,
            studentId,
            courseId,
            courseName,
            offeringId,
            className);
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
    private AcademicAchievementMathematicsNomination(
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

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        string courseName,
        Grade courseGrade,
        OfferingId offeringId,
        string className)
    {
        if (AwardType.AcademicAchievementMathematics.Grades.Count > 0 && !AwardType.AcademicAchievementMathematics.Grades.Contains(courseGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.AcademicAchievementMathematics, courseGrade));

        return new AcademicAchievementMathematicsNomination(
            periodId,
            studentId,
            courseId,
            courseName,
            offeringId,
            className);
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
    private AcademicAchievementScienceTechnologyNomination(
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

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        CourseId courseId,
        string courseName,
        Grade courseGrade,
        OfferingId offeringId,
        string className)
    {
        if (AwardType.AcademicAchievementScienceTechnology.Grades.Count > 0 && !AwardType.AcademicAchievementScienceTechnology.Grades.Contains(courseGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.AcademicAchievementScienceTechnology, courseGrade));

        return new AcademicAchievementScienceTechnologyNomination(
            periodId,
            studentId,
            courseId,
            courseName,
            offeringId,
            className);
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
    private PrincipalsAwardNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.PrincipalsAward;
    }

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        Grade studentGrade)
    {
        if (AwardType.PrincipalsAward.Grades.Count > 0 && !AwardType.PrincipalsAward.Grades.Contains(studentGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.PrincipalsAward, studentGrade));

        return new PrincipalsAwardNomination(
            periodId,
            studentId);
    }

    public override string ToString() => $"{AwardType.ToString()}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) => $"Principals Award";
}

public sealed class GalaxyMedalNomination : Nomination
{
    private GalaxyMedalNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.GalaxyMedal;
    }

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        Grade studentGrade)
    {
        if (AwardType.GalaxyMedal.Grades.Count > 0 && !AwardType.GalaxyMedal.Grades.Contains(studentGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.GalaxyMedal, studentGrade));

        return new GalaxyMedalNomination(
            periodId,
            studentId);
    }

    public override string ToString() => $"{AwardType.ToString()}";

    public override string GetDescription(bool showGrade = true, bool showClass = true) => $"Galaxy Medal";
}

public sealed class UniversalAchieverNomination : Nomination
{
    private UniversalAchieverNomination(
        AwardNominationPeriodId periodId,
        StudentId studentId)
    {
        Id = new();
        PeriodId = periodId;
        StudentId = studentId;
        AwardType = AwardType.UniversalAchiever;
    }

    public static Result<Nomination> Create(
        AwardNominationPeriodId periodId,
        StudentId studentId,
        Grade studentGrade)
    {
        if (AwardType.UniversalAchiever.Grades.Count > 0 && !AwardType.UniversalAchiever.Grades.Contains(studentGrade))
            return Result.Failure<Nomination>(AwardNominationErrors.InvalidGrade(AwardType.UniversalAchiever, studentGrade));

        return new UniversalAchieverNomination(
            periodId,
            studentId);
    }

    public override string ToString() => $"{AwardType.ToString()}";
    public override string GetDescription(bool showGrade = true, bool showClass = true) => $"Universal Achiever Award";
}