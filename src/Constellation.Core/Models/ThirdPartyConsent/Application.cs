#nullable enable
namespace Constellation.Core.Models.ThirdPartyConsent;

using Constellation.Core.Models.Students.Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationId = Identifiers.ApplicationId;

public sealed class Application : AggregateRoot, IAuditableEntity
{
    private readonly List<Consent> _consents = new();
    private readonly List<ConsentRequirement> _requirements = new();

    private Application() // Required by EF Core
    { } 

    private Application(
        string name,
        string purpose,
        string[] informationCollected,
        string storedCountry,
        string[] sharedWith,
        string applicationLink,
        bool consentRequired)
    {
        Name = name;
        Purpose = purpose;
        InformationCollected = informationCollected;
        StoredCountry = storedCountry;
        SharedWith = sharedWith;
        ApplicationLink = applicationLink;
        ConsentRequired = consentRequired;
    }

    public ApplicationId Id { get; private set; } = new();
    public string Name { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public string[] InformationCollected { get; private set; } = Array.Empty<string>();
    public string StoredCountry { get; private set; } = string.Empty;
    public string[] SharedWith { get; private set; } = Array.Empty<string>();
    public bool ConsentRequired { get; private set; }
    public string ApplicationLink { get; private set; } = string.Empty;
    public IReadOnlyList<Consent> Consents => _consents.ToList();
    public IReadOnlyList<ConsentRequirement> Requirements => _requirements.ToList();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Application Create(
        string name,
        string purpose,
        string[] informationCollected,
        string storedCountry,
        string[] sharedWith,
        string applicationLink,
        bool consentRequired)
    {
        Application application = new(
            name,
            purpose,
            informationCollected,
            storedCountry,
            sharedWith,
            applicationLink,
            consentRequired);

        return application;
    }

    public Result Update(
        string name,
        string purpose,
        string[] informationCollected,
        string storedCountry,
        string[] sharedWith,
        string applicationLink,
        bool consentRequired)
    {
        Name = name;
        Purpose = purpose;
        InformationCollected = informationCollected;
        StoredCountry = storedCountry;
        SharedWith = sharedWith;
        ApplicationLink = applicationLink;
        ConsentRequired = consentRequired;

        return Result.Success();
    }
    
    public List<Consent?> GetActiveConsents()
    {
        List<StudentId> studentIds = _consents
            .Select(consent => consent.StudentId)
            .Distinct()
            .ToList();
        
        return studentIds
            .Select(studentId => 
                _consents
                    .Where(consent => consent.StudentId == studentId)
                    .MaxBy(consent => consent.ProvidedAt))
            .ToList();
    }

    public void AddConsentResponse(Consent consent) => _consents.Add(consent);
    public void AddConsentRequirement(ConsentRequirement requirement) => _requirements.Add(requirement);

    public void Delete()
    {
        IsDeleted = true;

        foreach (ConsentRequirement requirement in _requirements.Where(entry => !entry.IsDeleted))
        {
            requirement.Delete();
        }
    }

    public void Reenable()
    {
        IsDeleted = false;
        DeletedAt = DateTime.MinValue;
        DeletedBy = string.Empty;
    }
}