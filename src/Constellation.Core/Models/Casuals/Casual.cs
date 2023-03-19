namespace Constellation.Core.Models.Casuals;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.ValueObjects;
using System;

public sealed class Casual : AggregateRoot, IAuditableEntity
{
    private Casual(
        CasualId id,
        string firstName,
        string lastName,
        string displayName,
        string emailAddress,
        string adobeConnectId,
        string schoolCode)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        EmailAddress = emailAddress;
        AdobeConnectId = adobeConnectId;
        SchoolCode = schoolCode;
    }

    public CasualId Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    public string EmailAddress { get; private set; }
    public string AdobeConnectId { get; private set; }
    public string SchoolCode { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Casual Create(
        CasualId id,
        Name name,
        EmailAddress email,
        string adobeConnectId,
        string schoolCode)
    {
        var casual = new Casual(id, name.FirstName, name.LastName, name.DisplayName, email.Email, adobeConnectId, schoolCode);

        // Raise domain events if necessary

        return casual;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = DateTime.MinValue;
    }

    public void Update(
        Name name,
        string adobeConnectId,
        string schoolCode)
    {
        FirstName = name.FirstName;
        LastName = name.LastName;
        DisplayName = name.DisplayName;
        AdobeConnectId = adobeConnectId;
        SchoolCode = schoolCode;
    }
}