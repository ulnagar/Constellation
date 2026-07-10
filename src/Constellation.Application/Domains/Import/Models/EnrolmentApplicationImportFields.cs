namespace Constellation.Application.Domains.Import.Models;

using Common.Errors;
using Core.Shared;
using Interfaces;
using System.Collections.Generic;

public static class EnrolmentApplicationImportFields
{
    public static readonly IReadOnlyList<ImportFieldDefinition> Definitions =
    [
        new("StudentReferenceNumber", "Student Reference Number (SRN)", Required: false, GroupLabel: "Student"),
        new("StudentName.First", "First Name", Required: true, GroupLabel: "Student"),
        new("StudentName.Preferred", "Preferred Name", Required: false, GroupLabel: "Student"),
        new("StudentName.Last", "Last Name", Required: true, GroupLabel: "Student"),
        new("DateOfBirth", "Date of Birth", Required: false, GroupLabel: "Student"),
        new("Gender", "Gender", Required: false, GroupLabel: "Student"),
        new("StudentEmailAddress", "Email Address", Required: false, GroupLabel: "Student"),
        new("ParentName.First", "First Name", Required: false, GroupLabel: "Parent"),
        new("ParentName.Last", "Last Name", Required: false, GroupLabel: "Parent"),
        new("ParentEmailAddress", "Email Address", Required: false, GroupLabel: "Parent"),
        new("ParentPhoneNumber", "Phone Number", Required: false, GroupLabel: "Parent"),
        new("MailingAddress.Street", "Street", Required: false, GroupLabel: "Address"),
        new("MailingAddress.Town", "Town", Required: false, GroupLabel: "Address"),
        new("MailingAddress.State", "State", Required: false, GroupLabel: "Address"),
        new("MailingAddress.Postcode", "Post Code", Required: false, GroupLabel: "Address"),
        new("ApplicationReference", "Reference", Required: false, GroupLabel: "Application"),
        new("CurrentSchoolName", "Current School", Required: false, GroupLabel: "Application"),
        new("DestinationSchoolName", "Destination School", Required: false, GroupLabel: "Application"),
        new("Grade", "Grade", Required: true, GroupLabel: "Application")
    ];
}
