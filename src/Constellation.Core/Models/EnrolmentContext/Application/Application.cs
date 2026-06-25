namespace Constellation.Core.Models.EnrolmentContext.Application;

using EnrolmentPeriod.Identifiers;
using Enums;
using Identifiers;
using Models.Identifiers;
using Offer.Enums;
using Students.Enums;
using Students.ValueObjects;
using ValueObjects;

public sealed class Application
{
    public Application()
    {
        Id = new();
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
    public string ApplicationReference { get; private set; }
    public SchoolCode? CurrentSchoolCode { get; private set; }
    public string CurrentSchool { get; private set; }
    public SchoolCode? DestinationSchoolCode { get; private set; }
    public string DestinationSchool { get; private set; }
    public Program Program { get; private set; }
    public Grade Grade { get; private set; }
}