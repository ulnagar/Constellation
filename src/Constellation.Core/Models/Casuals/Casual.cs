namespace Constellation.Core.Models.Casuals;

using Identifiers;
using Primitives;
using System;
using ValueObjects;

public sealed class Casual : AggregateRoot, IAuditableEntity
{
    /// <summary>
    /// For EF Core Use Only
    /// </summary>
    private Casual() { }

    private Casual(
        Name name,
        EmailAddress emailAddress,
        string schoolCode)
    {
        Id = new();
        Name = name;
        EmailAddress = emailAddress;
        SchoolCode = schoolCode;
    }

    public CasualId Id { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public string EdvalTeacherId { get; private set; }
    public string SchoolCode { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Casual Create(
        Name name,
        EmailAddress email,
        string schoolCode)
    {
        Casual casual = new Casual(name, email, schoolCode);

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
        string edvalTeacherId,
        string schoolCode)
    {
        Name = name;
        EdvalTeacherId = edvalTeacherId;
        SchoolCode = schoolCode;
    }
}