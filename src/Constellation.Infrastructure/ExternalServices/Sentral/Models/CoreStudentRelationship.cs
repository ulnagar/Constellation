namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Constellation.Infrastructure.ExternalServices.Sentral.Errors;
using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class CoreStudentRelationship
{
    public static Result<CoreStudentRelationship> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "coreStudentRelationship")
            return Result.Failure<CoreStudentRelationship>(SentralJsonErrors.IncorrectObject("CoreStudentRelationship", typeExists ? type.GetString() : string.Empty));

        CoreStudentRelationship relationship = new();
        relationship.Relationship = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            relationship.Relationship = attributes.ExtractString("relationship");
            relationship.IsResidentialGuardian = attributes.ExtractBool("isResidentialGuardian") ?? false;
            relationship.IsEmergencyContact = attributes.ExtractBool("isEmergencyContact") ?? false;
            relationship.IsActive = attributes.ExtractBool("isActive") ?? false;
            relationship.Sequence = attributes.ExtractInt("sequence");

            bool personExists = attributes.TryGetProperty("corePerson", out JsonElement person);
            if (personExists)
            {
                relationship.PersonId = person.ExtractString("id");
                relationship.FirstName = person.ExtractString("firstName");
                relationship.LastName = person.ExtractString("lastName");
                relationship.PreferredName = person.ExtractString("preferredName");
                relationship.Title = person.ExtractString("title");
                relationship.Gender = person.ExtractString("gender");
                relationship.Mobile = person.ExtractString("mobile");
                relationship.EmailAddress = person.ExtractString("email");
                relationship.ExternalId = person.ExtractGuid("externalId");
            }
        }

        bool relationshipsExists = jsonEntry.TryGetProperty("relationships", out JsonElement relationships);
        if (!relationshipsExists)
            return relationship;

        bool studentExists = relationships.TryGetProperty("coreStudent", out JsonElement student);
        if (!studentExists) 
            return relationship;

        relationship.StudentId = student.GetProperty("data").GetProperty("id").GetString();
        
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