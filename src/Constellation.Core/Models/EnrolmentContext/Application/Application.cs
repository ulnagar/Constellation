namespace Constellation.Core.Models.EnrolmentContext.Application;

using EnrolmentPeriod.Identifiers;
using Enums;
using Errors;
using Identifiers;
using Models.Identifiers;
using Offer.Enums;
using Shared;
using Students.Enums;
using Students.ValueObjects;
using ValueObjects;

public sealed class Application
{
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
    }

    /* Application Status and Tracking */
    public ApplicationId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; }


    /* Student Details */
    public StudentReferenceNumber? StudentReferenceNumber { get; private set; }
    public Name StudentName { get; private set; }
    public Gender StudentGender { get; private set; }
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
        if (!IsValidProgramGradeCombination(program, grade))
        {
            return Result.Failure<Application>(EnrolmentApplicationErrors.InvalidProgramGradeCombination(program, grade));
        }

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
}