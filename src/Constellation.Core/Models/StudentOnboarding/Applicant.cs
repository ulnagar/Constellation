namespace Constellation.Core.Models.StudentOnboarding;

using Common.Enums;
using Identifiers;
using Primitives;
using Students.ValueObjects;
using ValueObjects;

public sealed class Applicant : AggregateRoot, IAuditableEntity
{
    private Applicant()
    {
        Id = new();
    }

    public Applicant(
        StudentReferenceNumber? srn,
        Name name,
        EmailAddress? emailAddress,
        Gender? gender,
        IndigenousStatus indigenousStatus)
    {
        Id = new();

        StudentReferenceNumber = srn;
        Name = name;
        EmailAddress = emailAddress;
        Gender = gender;
        IndigenousStatus = indigenousStatus;
    }

    public ApplicantId Id { get; private set; }
    public StudentReferenceNumber? StudentReferenceNumber { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress? EmailAddress { get; private set; }
    public Gender? Gender { get; private set; }
    public IndigenousStatus IndigenousStatus { get; private set; } = IndigenousStatus.Unknown;
    
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}