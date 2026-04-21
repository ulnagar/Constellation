namespace Constellation.Core.Models.Assessments;

using Auth;
using Core.ValueObjects;
using Enums;
using Identifiers;
using Models.Identifiers;
using Primitives;
using Students;
using Students.Identifiers;

public sealed class AssessmentStudent : IAuditableEntity
{
    private readonly List<AssessmentProvision> _provisions = [];
    private readonly List<AssessmentSubmission> _submissions = [];

    /// <summary>
    /// Required for EF Core
    /// </summary>
    private AssessmentStudent() { }

    internal AssessmentStudent(
        AssessmentId assessmentId,
        Student student)
    {
        Id = new();

        AssessmentId = assessmentId;
        StudentId = student.Id;

        Student = student.Name;
        StudentGrade = student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram;
        SchoolCode = student.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty;
        SchoolName = student.CurrentEnrolment?.SchoolName ?? string.Empty;
    }

    public AssessmentStudentId Id { get; init; }
    public AssessmentId AssessmentId { get; private set; }
    public StudentId StudentId { get; private set; }
    
    public Name Student { get; private set; }
    public Grade StudentGrade { get; private set; }
    public SchoolCode SchoolCode { get; private set; }
    public string SchoolName { get; private set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public IReadOnlyList<AssessmentProvision> Provisions => _provisions.AsReadOnly();
    public IReadOnlyList<AssessmentSubmission> Submissions => _submissions.AsReadOnly();

    internal void AddProvision(Provision provision)
    {
        if (_provisions.Any(entry => entry.ProvisionId == provision.Id))
            return;

        _provisions.Add(new AssessmentProvision(provision));
    }

    internal SubmissionId AddSubmission(AppUser user)
    {
        AssessmentSubmission submission = new(Id, user);
        _submissions.Add(submission);
        return submission.Id;
    }

    internal void Delete() => IsDeleted = true;

    internal void Reinstate(List<Provision> provisions)
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = DateTime.MinValue;

        foreach (Provision provision in provisions)
            AddProvision(provision);
    }
}