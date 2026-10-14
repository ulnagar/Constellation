namespace Constellation.Core.Models.EnrolmentContext.Application;

using Core.Enums;
using EnrolmentParent;
using EnrolmentPeriod.Enums;
using EnrolmentPeriod.Identifiers;
using EnrolmentStudent.Identifiers;
using Enums;
using Errors;
using Identifiers;
using Models.Identifiers;
using Shared;

public sealed class Application
{
    private readonly List<CourseSelection> _courses = [];
    private readonly List<EnrolmentParent> _parents = [];

    /// <summary>
    /// DO NOT USE. EF CORE ONLY
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Application() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Application(
        EnrolmentPeriodId periodId,
        EnrolmentStudentId studentId,
        EnrolmentParent parent,
        Program program,
        Grade grade)
    {
        Id = new();

        PeriodId = periodId;
        Program = program;
        Grade = grade;

        StudentId = studentId;
        _parents.Add(parent);

        Status = ApplicationStatus.Pending;
    }

    /* Application Status and Tracking */
    public ApplicationId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; }

    /* Student Details */
    public EnrolmentStudentId StudentId { get; private set; }
    
    /* Parent Details */
    public IReadOnlyList<EnrolmentParent> Parents => _parents.AsReadOnly();

    /* Application Details */
    public string? ApplicationReference { get; private set; }
    public SchoolCode? CurrentSchoolCode { get; private set; }
    public string? CurrentSchool { get; private set; }
    public SchoolCode? DestinationSchoolCode { get; private set; }
    public string? DestinationSchool { get; private set; }
    public Program Program { get; private set; }
    public Grade Grade { get; private set; }
    public IReadOnlyList<CourseSelection> SelectedCourses => _courses.AsReadOnly();
    public ApplicationStatus Status { get; private set; }

    public static Result<Application> Create(
        EnrolmentPeriodId periodId,
        EnrolmentStudentId studentId,
        EnrolmentParent parent,
        string? applicationReference,
        SchoolCode? currentSchoolCode,
        string? currentSchool,
        SchoolCode? destinationSchoolCode,
        string? destinationSchool,
        Program program,
        Grade grade)
    {
        if (periodId == EnrolmentPeriodId.Empty)
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidEnrolmentPeriod);

        if (!IsValidProgramGradeCombination(program, grade))
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidProgramGradeCombination(program, grade));

        Application application = new(
            periodId,
            studentId, 
            parent, 
            program, 
            grade)
        {
            ApplicationReference = applicationReference,
            CurrentSchoolCode = currentSchoolCode,
            CurrentSchool = currentSchool,
            DestinationSchoolCode = destinationSchoolCode,
            DestinationSchool = destinationSchool
        };

        return application;
    }

    public Result Update(
        string applicationReference,
        SchoolCode? currentSchoolCode,
        string currentSchool,
        SchoolCode? destinationSchoolCode,
        string destinationSchool,
        Program program,
        Grade grade)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        if (!IsValidProgramGradeCombination(program, grade))
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidProgramGradeCombination(program, grade));

        ApplicationReference = applicationReference;
        CurrentSchoolCode = currentSchoolCode;
        CurrentSchool = currentSchool;
        DestinationSchoolCode = destinationSchoolCode;
        DestinationSchool = destinationSchool;
        Program = program;
        Grade = grade;

        return Result.Success();
    }

    public static bool IsValidProgramGradeCombination(Program program, Grade grade) =>
        (program, grade) switch
        {
            ({ Value: "OC" }, { Value: "Y05" }) => true,
            ({ Value: "SHS" }, { Value: "Y07" or "Y08" or "Y09" or "Y10" }) => true,
            ({ Value: "YDM" }, { Value: "Y05" or "Y06" or "Y07" or "Y08" or "Y09" or "Y10" }) => true,
            ({ Value: "S6R" }, { Value: "Y11" or "Y12" }) => true,
            ({ Value: "S6M" }, { Value: "Y11" or "Y12" }) => true,
            _ => false
        };

    public Result UpdateStatus(ApplicationStatus newStatus)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        if (Status == ApplicationStatus.Offered)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateOfferedApplication);

        Status = newStatus;

        if (newStatus == ApplicationStatus.Approved)
        {
            foreach (CourseSelection course in _courses.Where(course => course.Status == CourseSelectionStatus.Pending).ToList())
            {
                UpdateCourse(course.Course, course.Line, CourseSelectionStatus.Approved);
            }
        }

        return Result.Success();
    }

    public void AddParent(EnrolmentParent parent)
    {
        if (_parents.Any(entry => entry.Id == parent.Id))
            return;

        _parents.Add(parent);
    }

    public void RemoveParent(EnrolmentParent parent) =>
        _parents.Remove(parent);

    public void AddCourse(EnrolmentCourse course, string line)
    {
        if (_courses.Any(entry => entry.Course == course))
            return;

        _courses.Add(new(course, line));
    }

    public void RemoveCourse(EnrolmentCourse course) =>
        _courses.RemoveAll(entry => entry.Course == course);

    public Result UpdateCourse(EnrolmentCourse course, string line, CourseSelectionStatus status)
    {
        int index = _courses.FindIndex(entry => entry.Course == course);

        if (index < 0)
            return Result.Failure(EnrolmentApplicationErrors.InvalidCourse(course));

        _courses[index] = _courses[index] with { Line = line, Status = status };

        return Result.Success();
    }

    public Result UpdateGrade(Grade grade)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        if (!IsValidProgramGradeCombination(Program, grade))
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidProgramGradeCombination(Program, grade));

        Grade = grade;

        return Result.Success();
    }

    public Result UpdateCurrentSchool(SchoolCode? code, string? name)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        CurrentSchoolCode = code;
        CurrentSchool = name;

        return Result.Success();
    }

    public Result UpdateDestinationSchool(SchoolCode code, string name)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        DestinationSchoolCode = code;
        DestinationSchool = name;

        return Result.Success();
    }

    public Result UpdateApplicationReference(string? applicationReference)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        ApplicationReference = applicationReference;

        return Result.Success();
    }
}