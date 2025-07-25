﻿using Constellation.Core.Models.Students.Identifiers;

namespace Constellation.Core.Errors;

using Constellation.Core.Enums;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using System;

public static class DomainErrors
{
    public static class Absences
    {
        public static class Absence
        {
            public static readonly Error AlreadyExplained = new(
                "Absences.Absence.AlreadyExplained",
                "Cannot explain an absence that has already been explained");

            public static readonly Func<AbsenceId, Error> NotFound = id => new Error(
                "Absences.Absence.NotFound",
                $"Could not find any absence with the id {id}");
        }

        public static class Notification
        {
            public static readonly Func<AbsenceNotificationId, Error> NotFound = id => new(
                "Absences.Notification.NotFound",
                $"Could not find any absence notification with the Id {id}");
        }

        public static class Report
        {
            public static readonly Error NoFilterSupplied = new(
                "Absences.Report.NoFilterSupplied",
                "Cannot generate report without a supplied filter");
        }

        public static class Response
        {
            public static readonly Func<AbsenceResponseId, Error> NotFound = id => new Error(
                "Absences.Response.NotFound",
                $"Could not find any response with the id {id}");
        }
    }

    public static class Assets
    {
        public static class Allocations
        {
            public static readonly Func<StudentId, Error> NotFoundForStudent = id => new Error(
                "Assets.Allocations.NotFoundForStudent",
                $"Could not find any asset allocations for student {id}");
        }
    }

    public static class Auth
    {
        public static readonly Error UserNotFound = new(
            "Auth.UserNotFound",
            "Cannot find a user that matches the entry");

        public static readonly Error RoleNotFound = new(
            "Auth.RoleNotFound",
            "Cannot find a role that matches the entry");

        public static readonly Func<string, Error> CannotUpdateRole = (role) => new Error(
            "Auth.CannotUpdateRole",
            $"Cannot update role {role}");

        public static readonly Error NotAuthorised = new(
            "Auth.NotAuthorised",
            "The current user is not authorised to complete this action");
    }

    public static class Casuals
    {
        public static class Casual
        {
            public static readonly Func<CasualId, Error> NotFound = id => new Error(
                "Casuals.Casual.NotFound",
                $"A Casual with the Id {id.Value} could not be found");
        }
    }
    
    public static class Documents
    {
        public static class AwardCertificate
        {
            public static readonly Func<StudentAwardId, Error> NotFound = id => new(
                "Documents.AwardCertificate.NotFound",
                $"Could not find a document with the link id {id.Value}");
        }

        public static class AssignmentSubmission
        {
            public static readonly Func<string, Error> NotFound = id => new(
                "Documents.AssignmentSubmission.NotFound",
                $"Could not find a document with the link id {id}");
        }

        public static class AcademicReport
        {
            public static readonly Func<string, Error> NotFound = id => new(
                "Documents.AcademicReport.NotFound",
                $"Could not find a document with the link id {id}");
        }

        public static class TrainingCertificate
        {
            public static readonly Error NotFound = new(
                "Documents.TrainingCertificate.NotFound",
                "Could not find certificate in the database");
        }
    }

    public static class Enrolments
    {
        public static class Enrolment
        {
            public static readonly Func<StudentId, Error> NotFoundForStudent = id => new Error(
                "Enrolments.Enrolment.NotFoundForStudent",
                $"No enrolments could be found for student with Id {id}");

            public static readonly Func<StudentId, OfferingId, Error> AlreadyExists = (studentId, offeringId) => new(
                "Enrolments.Enrolment.AlreadyExists",
                $"A current enrolment already exists for student {studentId} and offering {offeringId}");
        }
    }

    public static class GroupTutorials
    {
        public static class GroupTutorial
        {
            public static readonly Error TutorialHasExpired = new(
                "GroupTutorials.GroupTutorial.TutorialHasExpired",
                "The Tutorial has already ended or has been deleted");

            public static readonly Func<GroupTutorialId, Error> NotFound = id => new Error(
                "GroupTutorials.GroupTutorial.NotFound",
                $"A tutorial with the Id {id.Value} could not be found");

            public static readonly Error CouldNotCreateTutorial = new(
                "GroupTutorials.GroupTutorial.CouldNotCreate",
                "There was an error attempting to create the group tutorial");
        }

        public static class TutorialTeacher
        {
            public static readonly Error NotFound = new(
                "GroupTutorials.TutorialTeacher.EntryNotFound",
                "There is no corresponding teacher record for that tutorial");

            public static readonly Error TimeExpired = new(
                "GroupTutorials.TutorialTeacher.TimeExpired",
                "The limited access window for this teacher has expired");
        }

        public static class TutorialEnrolment
        {
            public static readonly Error NotFound = new(
                "GroupTutorials.TutorialEnrolment.EntryNotFound",
                "There is no corresponding enrolment record for that tutorial");

            public static readonly Error StudentAlreadyEnrolled = new(
                "GroupTutorials.TutorialEnrolment.StudentAlreadyEnrolled",
                "The student is already actively enrolled in this tutorial");

            public static readonly Error TimeExpired = new(
                "GroupTutorials.TutorialEnrolment.TimeExpired",
                "The limited access window for this student has expired");
        }

        public static class TutorialRoll
        {
            public static readonly Func<DateOnly, Error> RollAlreadyExistsForDate = rollDate => new Error(
                "GroupTutorials.TutorialRoll.RollAlreadyExistsForDate",
                $"A roll for date {rollDate.ToShortDateString()} already exists");

            public static readonly Func<DateOnly, Error> RollDateInvalid = rollDate => new Error(
                "GroupTutorials.TutorialRoll.RollDateInvalid",
                $"Cannot create a roll for {rollDate.ToShortDateString()} as this is not a valid date for this tutorial");

            public static readonly Func<TutorialRollId, Error> NotFound = id => new Error(
                "GroupTutorials.TutorialRoll.NotFound",
                $"A roll with the Id {id.Value} could not be found");

            public static readonly Error SubmitInvalidStatus = new(
                "GroupTutorials.TutorialRoll.SubmitInvalidStatus",
                "Cannot submit a roll that has been cancelled or previously submitted");

            public static readonly Func<StudentId, Error> StudentNotFound = student => new Error(
                "GroupTutorials.TutorialRoll.StudentNotFound",
                $"Cannot find an attendance record for student with Id {student} attached to the roll");

            public static readonly Func<StudentId, Error> RemoveEnrolledStudent = student => new Error(
                "GroupTutorials.TutorialRoll.RemoveEnrolledStudent",
                $"Cannot remove student with Id {student} from the roll as they are enrolled in the tutorial");
        }

    }

    public static class LinkedSystems
    {
        public static class AdobeConnect
        {
            public static readonly Func<string, Error> RoomNotFound = id => new(
                "LinkedSystems.AdobeConnect.RoomNotFound",
                $"Could not find an Adobe Connect Room with the Id {id}");

            public static readonly Error CannotCreate = new(
                "LinkedSystems.AdobeConnect.CannotCreate",
                "An error occured while waiting for Adobe Connect to create the room");
        }

        public static class Sentral
        {
            public static readonly Func<string, Error> FamilyIdNotValid = id => new Error(
                "LinkedSystems.Sentral.FamilyIdNotValid",
                $"The value {id} is not a valid Sentral Family Id.");
        }

        public static class Teams
        {
            public static readonly Error TeamNotFoundInDatabase = new(
                "LinkedSystems.Teams.TeamNotFoundInDatabase",
                "The Team could not be found in the database");

            public static readonly Error MoreThanOneMatchFound = new(
                "LinkedSystems.Teams.MoreThanOneMatchFound",
                "Found more than one Team that matched the criteria in the database");

            public static readonly Func<Guid, Error> AlreadyExists = id => new Error(
                "LinkedSystems.Teams.AlreadyExists",
                $"The Team with Id {id} could not be created because it already exists in the database");
        }
    }

    public static class Partners
    {
        public static class Contact
        {
            public static readonly Func<int, Error> NotFound = id => new(
                "Partners.Contact.NotFound",
                $"Could not find a School Contact with the Id {id}");
        }

        public static class Faculty
        {
            public static readonly Func<Guid, Error> NotFound = id => new(
                "Partners.Faculty.NotFound",
                $"Could not find a Faculty with the Id {id}");
        }

        public static class School
        {
            public static readonly Func<string, Error> NotFound = id => new(
                "Partners.School.NotFound",
                $"A school with the code {id} could not be found");
        }
    }



    public static class Permissions
    {
        public static readonly Error Unauthorised = new(
            "Permissions.Unauthorised",
            "You do not have the required permissions to perform this action");
    }

    public static class ValueObjects
    {
        public static class EmailAddress
        {
            public static readonly Error EmailEmpty = new(
                "ValueObjects.EmailAddress.EmailEmpty",
                "Email Address must not be empty.");

            public static readonly Error EmailInvalid = new(
                "ValueObjects.EmailAddress.EmailInvalid",
                "Email Address is not valid.");
        }

        public static class EmailRecipient
        {
            public static readonly Error NameEmpty = new(
                "ValueObjects.EmailRecipient.NameEmpty",
                "Email Recipient must have a valid name.");
        }

        public static class Name
        {
            public static readonly Error FirstNameEmpty = new(
                "ValueObjects.Name.FirstNameEmpty",
                "First Name must not be empty.");

            public static readonly Error LastNameEmpty = new(
                "ValueObjects.Name.LastNameEmpty",
                "Last Name must not be empty.");
        }

        public static class OfferingName
        {
            public static readonly Func<Grade, Error> InvalidGrade = grade => new(
                "ValueObjects.OfferingName.InvalidGrade",
                $"Invalid grade supplied: {grade}");

            public static readonly Func<string, Error> InvalidCourseCode = code => new(
                "ValueObjects.OfferingName.InvalidCourseCode",
                $"Invalid course code supplied: {code}");

            public static readonly Func<string, Error> InvalidTutorialInitials = initials => new(
                "ValueObjects.OfferingName.InvalidTutorialInitials",
                $"Invalid initals supplied for tutorial class: {initials}");
        }

        public static class PhoneNumber
        {
            public static readonly Error NumberEmpty = new(
                "ValueObjects.PhoneNumber.NumberEmpty",
                "Phone Number must not be empty");

            public static readonly Error NumberInvalid = new(
                "ValueObjects.PhoneNumber.NumberInvalid",
                "Phone NUmber is not valid");
        }
    }

    public static class Operations
    {
        public static class Canvas
        {
            public static readonly Error Invalid = new(
                "Operations.Canvas.Invalid",
                "Failed to find valid code path for Canvas Operation");

            public static readonly Func<int, Error> NotFound = id => new(
                "Operations.Canvas.NotFound",
                $"Could not find a Canvas Operation with the Id {id}");

            public static readonly Error ProcessFailed = new(
                "Operations.Canvas.ProcessFailed",
                "Failed to process the Canvas Operation");
        }
    }
}