namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;

public sealed class CoreStudentPersonRelation
{
    public static Result<CoreStudentPersonRelation> ConvertFromJson(dynamic jsonEntry)
    {
        if (jsonEntry["type"].ToString() != "coreStudent")
            return Result.Failure<CoreStudentPersonRelation>(SentralJsonErrors.IncorrectObject("CoreStudentPersonRelation", jsonEntry["type"].ToString()));

        CoreStudentPersonRelation relationship = new()
        {
            RelationshipId = jsonEntry["id"].ToString(),
            PersonId = jsonEntry["relationships"]["corePerson"]["data"]["id"].ToString(),
            StudentId = jsonEntry["relationships"]["coreStudent"]["data"]["id"].ToString(),
            Relationship = jsonEntry["attributes"]["relationship"].ToString()
        };

        bool guardianSuccess = bool.TryParse(jsonEntry["attributes"]["isResidentialGuardian"].ToString(), out bool isResidentialGuardian);
        if (guardianSuccess)
            relationship.IsResidentialGuardian = isResidentialGuardian;

        bool emergencyContactSuccess = bool.TryParse(jsonEntry["attributes"]["isEmergencyContact"].ToString(), out bool isEmergencyContact);
        if (emergencyContactSuccess)
            relationship.IsEmergencyContact = isEmergencyContact;

        bool contactSmsSuccess = bool.TryParse(jsonEntry["attributes"]["canContactViaSms"].ToString(), out bool canContactViaSms);
        if (contactSmsSuccess)
            relationship.CanContactViaSms = canContactViaSms;

        bool contactEmailSuccess = bool.TryParse(jsonEntry["attributes"]["canContactViaEmail"].ToString(), out bool canContactViaEmail);
        if (contactEmailSuccess)
            relationship.CanContactViaEmail = canContactViaEmail;

        bool contactPhoneSuccess = bool.TryParse(jsonEntry["attributes"]["canContactViaPhone"].ToString(), out bool canContactViaPhone);
        if (contactPhoneSuccess)
            relationship.CanContactViaPhone = canContactViaPhone;

        bool contactLetterSuccess = bool.TryParse(jsonEntry["attributes"]["canContactViaLetter"].ToString(), out bool canContactViaLetter);
        if (contactLetterSuccess)
            relationship.CanContactViaLetter = canContactViaLetter;

        bool correspondenceSuccess = bool.TryParse(jsonEntry["attributes"]["canReceiveCorrespondence"].ToString(), out bool canReceiveCorrespondence);
        if (correspondenceSuccess)
            relationship.CanReceiveCorrespondence = canReceiveCorrespondence;

        bool portalAccessSuccess = bool.TryParse(jsonEntry["attributes"]["canReceivePortalAccess"].ToString(), out bool canReceivePortalAccess);
        if (portalAccessSuccess)
            relationship.CanReceivePortalAccess = canReceivePortalAccess;

        bool reportSuccess = bool.TryParse(jsonEntry["attributes"]["canReceiveReports"].ToString(), out bool canReceiveReports);
        if (reportSuccess)
            relationship.CanReceiveReports = canReceiveReports;

        bool absencesSuccess = bool.TryParse(jsonEntry["attributes"]["canReceiveAbsences"].ToString(), out bool canReceiveAbsences);
        if (absencesSuccess)
            relationship.CanReceiveAbsences = canReceiveAbsences;

        bool smsSuccess = bool.TryParse(jsonEntry["attributes"]["canReceiveSms"].ToString(), out bool canReceiveSms);
        if (smsSuccess)
            relationship.CanReceiveSms = canReceiveSms;

        bool contactSuccess = bool.TryParse(jsonEntry["attributes"]["doNotContact"].ToString(), out bool doNotContact);
        if (contactSuccess)
            relationship.DoNotContact = doNotContact;

        bool sequenceSuccess = Int32.TryParse(jsonEntry["attributes"]["sequence"].ToString(), out int sequence);
        if (sequenceSuccess)
            relationship.Sequence = sequence;


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