namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Constellation.Core.Models.Students;
using Core.Shared;

public sealed class CoreStudentRelationship
{
    public static Result<CoreStudentRelationship> ConvertFromJson(dynamic jsonEntry)
    {
        if (jsonEntry["type"].ToString() != "coreStudentRelationship")
            return Result.Failure<CoreStudentRelationship>(SentralJsonErrors.IncorrectObject("CoreStudentRelationship", jsonEntry["type"].ToString()));

        CoreStudentRelationship relationship = new()
        {
            RelationshipId = jsonEntry["id"].ToString(),
            PersonId = jsonEntry["attributes"]["corePerson"]["id"].ToString(),
            StudentId = jsonEntry["relationships"]["coreStudent"]["data"]["id"].ToString(),
            Relationship = jsonEntry["attributes"]["relationship"].ToString(),
            FirstName = jsonEntry["attributes"]["corePerson"]["firstName"].ToString(),
            LastName = jsonEntry["attributes"]["corePerson"]["lastName"].ToString(),
            PreferredName = jsonEntry["attributes"]["corePerson"]["preferredName"].ToString(),
            Title = jsonEntry["attributes"]["corePerson"]["title"].ToString(),
            Gender = jsonEntry["attributes"]["corePerson"]["gender"].ToString(),
            Mobile = jsonEntry["attributes"]["corePerson"]["mobile"].ToString(),
            EmailAddress = jsonEntry["attributes"]["corePerson"]["email"].ToString()
        };

        bool externalIdSuccess = Guid.TryParse(jsonEntry["attributes"]["corePerson"]["externalId"].ToString(), out Guid externalId);
        if (externalIdSuccess)
            relationship.ExternalId = externalId;


        bool guardianSuccess = bool.TryParse(jsonEntry["attributes"]["isResidentialGuardian"].ToString(), out bool isResidentialGuardian);
        if (guardianSuccess)
            relationship.IsResidentialGuardian = isResidentialGuardian;

        bool emergencyContactSuccess = bool.TryParse(jsonEntry["attributes"]["isEmergencyContact"].ToString(), out bool isEmergencyContact);
        if (emergencyContactSuccess)
            relationship.IsEmergencyContact = isEmergencyContact;

        bool sequenceSuccess = Int32.TryParse(jsonEntry["attributes"]["sequence"].ToString(), out int sequence);
        if (sequenceSuccess)
            relationship.Sequence = sequence;

        bool activeSuccess = bool.TryParse(jsonEntry["attributes"]["isActive"].ToString(), out bool isActive);
        if (activeSuccess)
            relationship.IsActive = isActive;

        return relationship;
    }

    public string RelationshipId { get; private set; }
    public string PersonId { get; private set; }
    public string StudentId { get; private set; }
    public string Relationship { get; private set; }
    public bool IsResidentialGuardian { get; private set; }
    public bool IsEmergencyContact { get; private set; }
    public int Sequence { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Title { get; private set; }
    public string PreferredName { get; private set; }
    public string Gender { get; private set; }
    public string Mobile { get; private set; }
    public string EmailAddress { get; private set; }
    public Guid ExternalId { get; private set; }
    public bool IsActive { get; private set; }
}