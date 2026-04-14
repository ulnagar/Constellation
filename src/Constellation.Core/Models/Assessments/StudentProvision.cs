namespace Constellation.Core.Models.Assessments;

using Core.ValueObjects;
using Identifiers;
using Primitives;
using Students;
using Students.Identifiers;
using ValueObjects;

public sealed class StudentProvision : IAuditableEntity
{
    /// <summary>
    /// Required for EF Core
    /// </summary>
    private StudentProvision() { }

    public StudentProvision(
        Provision provision,
        Student student,
        int year)
    {
        Id = new();

        ProvisionId = provision.Id;
        ProvisionCode = provision.Code;
        ProvisionDescription = provision.Description;

        StudentId = student.Id;
        Student = student.Name;

        Year = year;
    }

    public StudentProvisionId Id { get; init; }
    public ProvisionId ProvisionId { get; private set; }
    public ProvisionCode ProvisionCode { get; private set; }
    public string ProvisionDescription { get; private set; }

    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }

    public int Year { get; private set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void Delete() => IsDeleted = true;
}