#nullable enable
namespace Constellation.Core.Models.ThirdPartyConsent;

using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Extensions;
using Identifiers;
using Primitives;
using Shared;
using Students;
using Subjects;
using System;
using ApplicationId = Identifiers.ApplicationId;

public abstract class ConsentRequirement : IAuditableEntity
{
    public ConsentRequirementId Id { get; protected set; } = new();
    public ApplicationId ApplicationId { get; protected set; }
    public string ApplicationName { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public void Delete() => IsDeleted = true;
}

public sealed class GradeConsentRequirement : ConsentRequirement
{
    private GradeConsentRequirement() {} // Required by EF Core

    private GradeConsentRequirement(
        Grade grade,
        ApplicationId applicationId,
        string applicationName,
        string description)
    {
        Grade = grade;
        ApplicationId = applicationId;
        ApplicationName = applicationName;
        Description = description;
    }

    public Grade Grade { get; private set; }

    public static Result<GradeConsentRequirement> Create(
        Grade grade,
        Application application)
    {
        GradeConsentRequirement requirement = new(
            grade,
            application.Id,
            application.Name,
            grade.AsName());

        return requirement;
    }
}

public sealed class CourseConsentRequirement : ConsentRequirement
{
    private CourseConsentRequirement(
        CourseId courseId,
        ApplicationId applicationId,
        string applicationName,
        string description)
    {
        CourseId = courseId;
        ApplicationId = applicationId;
        ApplicationName = applicationName;
        Description = description;
    }

    public CourseId CourseId { get; private set; }

    public static Result<CourseConsentRequirement> Create(
        Course course,
        Application application)
    {
        CourseConsentRequirement requirement = new(
            course.Id,
            application.Id,
            application.Name,
            course.ToString());

        return requirement;
    }
}

public sealed class StudentConsentRequirement : ConsentRequirement
{
    private StudentConsentRequirement(
        StudentId studentId,
        ApplicationId applicationId,
        string applicationName,
        string description)
    {
        StudentId = studentId;
        ApplicationId = applicationId;
        ApplicationName = applicationName;
        Description = description;
    }

    public StudentId StudentId { get; private set; }

    public static Result<StudentConsentRequirement> Create(
        Student student,
        Application application)
    {
        StudentConsentRequirement requirement = new(
            student.Id,
            application.Id,
            application.Name,
            student.Name.DisplayName);

        return requirement;
    }
}