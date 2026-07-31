namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplication;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Enums;
using Constellation.Core.Models.Students.ValueObjects;
using Constellation.Core.ValueObjects;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using System;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record UpdateEnrolmentApplicationCommand(
    ApplicationId ApplicationId,
    StudentReferenceNumber? StudentReferenceNumber,
    Name StudentName,
    Gender StudentGender,
    DateOnly? DateOfBirth,
    EmailAddress? StudentEmailAddress,
    Name? ParentName,
    EmailAddress? ParentEmailAddress,
    PhoneNumber? ParentPhoneNumber,
    MailingAddress? MailingAddress,
    string ApplicationReference,
    SchoolCode? CurrentSchoolCode,
    string CurrentSchool,
    SchoolCode? DestinationSchoolCode,
    string DestinationSchool,
    Program Program,
    Grade Grade,
    List<EnrolmentCourse> Courses)
    : ICommand;