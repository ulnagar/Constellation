namespace Constellation.Core.Models.EnrolmentContext.Application;

using Constellation.Core.Models.EnrolmentContext.Application.Enums;
using Core.Enums;
using EnrolmentPeriod.Enums;
using EnrolmentPeriod.Identifiers;
using Errors;
using Identifiers;
using Models.Identifiers;
using Shared;
using Students.Enums;
using Students.ValueObjects;
using ValueObjects;

public sealed class Application
{
    private readonly List<CourseSelection> _courses = [];

    /// <summary>
    /// DO NOT USE. EF CORE ONLY
    /// </summary>
    private Application() { }

    private Application(
        EnrolmentPeriodId periodId,
        Name studentName,
        Gender studentGender,
        Program program,
        Grade grade)
    {
        Id = new();

        PeriodId = periodId;
        StudentName = studentName;
        StudentGender = studentGender;
        Program = program;
        Grade = grade;

        Status = ApplicationStatus.Pending;
    }

    /* Application Status and Tracking */
    public ApplicationId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; }


    /* Student Details */
    public StudentReferenceNumber? StudentReferenceNumber { get; private set; }
    public Name StudentName { get; private set; }
    public Gender StudentGender { get; private set; } = Gender.Empty;
    public DateOnly? DateOfBirth { get; private set; }
    public EmailAddress? StudentEmailAddress { get; private set; }
    
    /* Parent Details */
    public Name? ParentName { get; private set; }
    public EmailAddress? ParentEmailAddress { get; private set; }
    public PhoneNumber? ParentPhoneNumber { get; private set; }
    public MailingAddress? MailingAddress { get; private set; }

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
        StudentReferenceNumber? studentReferenceNumber,
        Name studentName,
        Gender studentGender,
        DateOnly? dateOfBirth,
        EmailAddress? studentEmailAddress,
        Name? parentName,
        EmailAddress? parentEmailAddress,
        PhoneNumber? parentPhoneNumber,
        MailingAddress? mailingAddress,
        string? applicationReference,
        SchoolCode? currentSchoolCode,
        string? currentSchool,
        SchoolCode? destinationSchoolCode,
        string? destinationSchool,
        Program program,
        Grade grade)
    {
        if (periodId == EnrolmentPeriodId.Empty)
        {
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidEnrolmentPeriod);
        }

        if (!IsValidProgramGradeCombination(program, grade))
        {
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidProgramGradeCombination(program, grade));
        }

        Application application = new(
            periodId,
            studentName, 
            studentGender, 
            program, 
            grade)
        {
            StudentReferenceNumber = studentReferenceNumber,
            StudentEmailAddress = studentEmailAddress,
            DateOfBirth = dateOfBirth,
            ParentName = parentName,
            ParentEmailAddress = parentEmailAddress,
            ParentPhoneNumber = parentPhoneNumber,
            MailingAddress = mailingAddress,
            ApplicationReference = applicationReference,
            CurrentSchoolCode = currentSchoolCode,
            CurrentSchool = currentSchool,
            DestinationSchoolCode = destinationSchoolCode,
            DestinationSchool = destinationSchool
        };

        return application;
    }

    public Result Update(
        StudentReferenceNumber? studentReferenceNumber,
        Name studentName,
        Gender studentGender,
        DateOnly? dateOfBirth,
        EmailAddress? studentEmailAddress,
        Name? parentName,
        EmailAddress? parentEmailAddress,
        PhoneNumber? parentPhoneNumber,
        MailingAddress? mailingAddress,
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

        StudentReferenceNumber = studentReferenceNumber;
        StudentName = studentName;
        StudentGender = studentGender;
        StudentEmailAddress = studentEmailAddress;
        DateOfBirth = dateOfBirth;
        ParentName = parentName;
        ParentEmailAddress = parentEmailAddress;
        ParentPhoneNumber = parentPhoneNumber;
        MailingAddress = mailingAddress;
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
            ({ Value: "OC" }, Grade.Y05) => true,
            ({ Value: "SHS" }, Grade.Y07 or Grade.Y08 or Grade.Y09 or Grade.Y10) => true,
            ({ Value: "YDM" }, Grade.Y05 or Grade.Y06 or Grade.Y07 or Grade.Y08 or Grade.Y09 or Grade.Y10) => true,
            ({ Value: "S6R" }, Grade.Y11 or Grade.Y12) => true,
            ({ Value: "S6M" }, Grade.Y11 or Grade.Y12) => true,
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
                UpdateCourse(course.Course, CourseSelectionStatus.Approved);
            }
        }

        return Result.Success();
    }

    public void AddCourse(EnrolmentCourse course)
    {
        if (_courses.Any(entry => entry.Course == course))
            return;

        _courses.Add(new(course));
    }

    public void RemoveCourse(EnrolmentCourse course) =>
        _courses.RemoveAll(entry => entry.Course == course);

    public Result UpdateCourse(EnrolmentCourse course, CourseSelectionStatus status)
    {
        int index = _courses.FindIndex(entry => entry.Course == course);

        if (index < 0)
            return Result.Failure(new Error(
                "Application.CourseNotSelected",
                $"{course.Name} is not currently selected on this application."));

        _courses[index] = _courses[index] with { Status = status };

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

    public Result UpdateStudentName(Name studentName)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        StudentName = studentName;

        return Result.Success();
    }

    public Result UpdateSRN(StudentReferenceNumber srn)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        StudentReferenceNumber = srn;

        return Result.Success();
    }

    public Result UpdateParentName(Name parentName)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        ParentName = parentName;

        return Result.Success();
    }

    public Result UpdateDateOfBirth(DateOnly dateOfBirth)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        DateOfBirth = dateOfBirth;

        return Result.Success();
    }

    public Result UpdateGender(Gender gender)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        StudentGender = gender;

        return Result.Success();
    }

    public Result UpdateStudentEmail(EmailAddress studentEmail)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        StudentEmailAddress = studentEmail;

        return Result.Success();
    }

    public Result UpdateParentEmail(EmailAddress parentEmail)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        ParentEmailAddress = parentEmail;

        return Result.Success();
    }

    public Result UpdateParentPhone(PhoneNumber parentPhone)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        ParentPhoneNumber = parentPhone;

        return Result.Success();
    }

    public Result UpdateMailingAddress(MailingAddress address)
    {
        if (Status == ApplicationStatus.Archived)
            return Result.Failure(EnrolmentApplicationErrors.CannotUpdateArchivedApplication);

        MailingAddress = address;

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