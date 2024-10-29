namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class CoreStudentPersonRelation
{
    public static Result<CoreStudentPersonRelation> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "coreStudentPersonRelation")
            return Result.Failure<CoreStudentPersonRelation>(SentralJsonErrors.IncorrectObject("CoreStudentPersonRelation", typeExists ? type.GetString() : string.Empty));

        CoreStudentPersonRelation relationship = new();
        relationship.RelationshipId = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            relationship.Relationship = attributes.ExtractString("relationship");
            relationship.IsResidentialGuardian = attributes.ExtractBool("isResidentialGuardian") ?? false;
            relationship.IsEmergencyContact = attributes.ExtractBool("isEmergencyContact") ?? false;
            relationship.CanContactViaSms = attributes.ExtractBool("canContactViaSms") ?? false;
            relationship.CanContactViaEmail = attributes.ExtractBool("canContactViaEmail") ?? false;
            relationship.CanContactViaPhone = attributes.ExtractBool("canContactViaPhone") ?? false;
            relationship.CanContactViaLetter = attributes.ExtractBool("canContactViaLetter") ?? false;
            relationship.CanReceiveCorrespondence = attributes.ExtractBool("canReceiveCorrespondence") ?? false;
            relationship.CanReceivePortalAccess = attributes.ExtractBool("canReceivePortalAccess") ?? false;
            relationship.CanReceiveReports = attributes.ExtractBool("canReceiveReports") ?? false;
            relationship.CanReceiveAbsences = attributes.ExtractBool("canReceiveAbsences") ?? false;
            relationship.CanReceiveSms = attributes.ExtractBool("canReceiveSms") ?? false;
            relationship.DoNotContact = attributes.ExtractBool("doNotContact") ?? false;
            relationship.Sequence = attributes.ExtractInt("sequence");
        }

        bool relationshipsExists = jsonEntry.TryGetProperty("relationship", out JsonElement relationships);
        if (!relationshipsExists)
            return relationship;

        bool personExists = relationships.TryGetProperty("corePerson", out JsonElement person);
        if (personExists)
            relationship.PersonId = person.GetProperty("data").GetProperty("id").GetString();
    
        bool studentExists = relationships.TryGetProperty("coreStudent", out JsonElement student);
        if (studentExists)
            relationship.StudentId = student.GetProperty("data").GetProperty("id").GetString();
    
        return relationship;
    }

    public string RelationshipId { get; private set; }
    public string StudentId { get; private set; }
    public string PersonId { get; private set; }
    public string Relationship { get; private set; }
    public int Sequence { get; private set; }
    public bool IsResidentialGuardian { get; private set; }
    public bool IsEmergencyContact { get; private set; }
    public bool CanContactViaSms { get; private set; }
    public bool CanContactViaEmail { get; private set; }
    public bool CanContactViaPhone { get; private set; }
    public bool CanContactViaLetter { get; private set; }
    public bool CanReceiveCorrespondence { get; private set; }
    public bool CanReceivePortalAccess { get; private set; }
    public bool CanReceiveReports { get; private set; }
    public bool CanReceiveSms { get; private set; }
    public bool CanReceiveAbsences { get; private set; }
    public bool DoNotContact { get; private set; }
}