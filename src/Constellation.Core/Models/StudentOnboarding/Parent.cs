namespace Constellation.Core.Models.StudentOnboarding;

using Identifiers;
using Primitives;
using ValueObjects;

public sealed class Parent : IAuditableEntity
{
    private readonly List<Application> _applications = [];

    private Parent()
    {
        Id = new();
    }

    public ParentId Id { get; private set; }
    public Name Name { get; private set; }
    public PhoneNumber? MobileNumber { get; private set; }
    public EmailAddress? EmailAddress { get; private set; }
    public MailingAddress? MailingAddress { get; private set; }

    public IReadOnlyList<Application> Applications => _applications.AsReadOnly();

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}