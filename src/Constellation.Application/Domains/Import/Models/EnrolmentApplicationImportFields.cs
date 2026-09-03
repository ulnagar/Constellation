namespace Constellation.Application.Domains.Import.Models;

using Common.Errors;
using Core.Shared;
using Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class EnrolmentApplicationImportFields
{
    public const string StudentReferenceNumber = "StudentReferenceNumber";
    public const string StudentNameFirst = "StudentName.First";
    public const string StudentNamePreferred = "StudentName.Preferred";
    public const string StudentNameLast = "StudentName.Last";
    public const string DateOfBirth = "DateOfBirth";
    public const string Gender = "Gender";
    public const string StudentEmailAddress = "StudentEmailAddress";
    public const string ParentNameFirst = "ParentName.First";
    public const string ParentNameLast = "ParentName.Last";
    public const string ParentEmailAddress = "ParentEmailAddress";
    public const string ParentPhoneNumber = "ParentPhoneNumber";
    public const string MailingAddressStreet = "MailingAddress.Street";
    public const string MailingAddressTown = "MailingAddress.Town";
    public const string MailingAddressState = "MailingAddress.State";
    public const string MailingAddressPostcode = "MailingAddress.Postcode";
    public const string ApplicationReference = "ApplicationReference";
    public const string CurrentSchoolName = "CurrentSchoolName";
    public const string DestinationSchoolName = "DestinationSchoolName";
    public const string Grade = "Grade";
    public const string Subjects = "Subjects";

    public static readonly IReadOnlyList<ImportFieldDefinition> Definitions =
    [
        new(StudentReferenceNumber, "Student Reference Number (SRN)", Required: false, GroupLabel: "Student"),
        new(StudentNameFirst, "First Name", Required: true, GroupLabel: "Student"),
        new(StudentNamePreferred, "Preferred Name", Required: false, GroupLabel: "Student"),
        new(StudentNameLast, "Last Name", Required: true, GroupLabel: "Student"),
        new(DateOfBirth, "Date of Birth", Required: false, GroupLabel: "Student"),
        new(Gender, "Gender", Required: false, GroupLabel: "Student"),
        new(StudentEmailAddress, "Email Address", Required: false, GroupLabel: "Student"),
        new(ParentNameFirst, "First Name", Required: false, GroupLabel: "Parent"),
        new(ParentNameLast, "Last Name", Required: false, GroupLabel: "Parent"),
        new(ParentEmailAddress, "Email Address", Required: false, GroupLabel: "Parent"),
        new(ParentPhoneNumber, "Phone Number", Required: false, GroupLabel: "Parent"),
        new(MailingAddressStreet, "Street", Required: false, GroupLabel: "Address"),
        new(MailingAddressTown, "Town", Required: false, GroupLabel: "Address"),
        new(MailingAddressState, "State", Required: false, GroupLabel: "Address"),
        new(MailingAddressPostcode, "Post Code", Required: false, GroupLabel: "Address"),
        new(ApplicationReference, "Reference", Required: false, GroupLabel: "Application"),
        new(CurrentSchoolName, "Current School", Required: false, GroupLabel: "Application"),
        new(DestinationSchoolName, "Destination School", Required: false, GroupLabel: "Application"),
        new(Grade, "Grade", Required: true, GroupLabel: "Application"),
        new(Subjects, "Subjects", Required: false, GroupLabel: "Application")
    ];
}
