namespace Constellation.Core.Models.StudentOnboarding;

using Common.Enums;
using Constellation.Core.Primitives;
using Core.Enums;
using Enums;
using Identifiers;
using Models.Identifiers;
using Students.ValueObjects;
using System;
using System.Collections.Generic;
using ValueObjects;
using ParentId = Models.Identifiers.ParentId;

public sealed class Applicant : AggregateRoot, IAuditableEntity
{
    private readonly List<Parent> _parents = [];

    private Applicant()
    {
        Id = new();
    }

    public ApplicantId Id { get; private set; }
    public StudentReferenceNumber? StudentReferenceNumber { get; private set; }
    public Name Name { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public Gender Gender { get; private set; }
    public IndigenousStatus IndigenousStatus { get; private set; }

    // Application specific
    public Program Program { get; private set; }
    public string Year { get; private set; }
    public Grade Grade { get; private set; }
    public SchoolCode SchoolCode { get; private set; }
    public string SchoolName { get; private set; }

    // Family details
    public IReadOnlyList<Parent> Parents => _parents.AsReadOnly();

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}

public sealed class Parent
{
    private Parent()
    {
        Id = new();
    }

    public ParentId Id { get; private set; }
    public ApplicantId ApplicantId { get; private set; }
    public string Title { get; private set; }
    public Name Name { get; private set; }
    public PhoneNumber MobileNumber { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
}


/*
 * x SRN
 * Year_started
 * x Student_First
 * x Student_Last
 * x Student_name
 * x Cohort
 * x School
 * Subject(s)
 * Parent First Name
 * Parent Last Name
 * Family
 * Home_Address_1
 * Home_Address_2
 * Parent_Mobile 1
 * Parent_Mobile 2
 * Parent_Email
 * Parent_email_2
 * x Student_Email
 */