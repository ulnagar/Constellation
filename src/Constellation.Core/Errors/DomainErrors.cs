namespace Constellation.Core.Errors;

using Constellation.Core.Shared;
using System;

public static class DomainErrors
{
    public static class Auth
    {
        public static readonly Error UserNotFound = new Error(
            "Auth.UserNotFound",
            "Cannot find a user that matches the entry");

        public static readonly Error RoleNotFound = new Error(
            "Auth.RoleNotFound",
            "Cannot find a role that matches the entry");

        public static readonly Func<string, Error> CannotUpdateRole = (role) => new Error(
            "Auth.CannotUpdateRole",
            $"Cannot update role {role}");
    }

    public static class Casuals
    {
        public static class Casual
        {
            public static readonly Func<Guid, Error> NotFound = id => new Error(
                "Casuals.Casual.NotFound",
                $"A Casual with the Id {id} could not be found");
        }
    }

    public static class ClassCovers
    {
        public static class Cover
        {
            public static readonly Func<Guid, Error> NotFound = id => new Error(
                "ClassCovers.Cover.NotFound",
                $"A Class Cover with the Id {id} could not be found");
        }
    }

    public static class GroupTutorials
    {
        public static class GroupTutorial
        {
            public static readonly Error TutorialHasExpired = new(
                "GroupTutorials.GroupTutorial.TutorialHasExpired",
                "The Tutorial has already ended or has been deleted");

            public static readonly Func<Guid, Error> NotFound = id => new Error(
                "GroupTutorials.GroupTutorial.NotFound",
                $"A tutorial with the Id {id} could not be found");

            public static readonly Error CouldNotCreateTutorial = new(
                "GroupTutorials.GroupTutorial.CouldNotCreate",
                "There was an error attempting to create the group tutorial");
        }

        public static class TutorialTeacher
        {
            public static readonly Error NotFound = new(
                "GroupTutorials.TutorialTeacher.EntryNotFound",
                "There is no corresponding teacher record for that tutorial");
        }

        public static class TutorialEnrolment
        {
            public static readonly Error NotFound = new(
                "GroupTutorials.TutorialEnrolment.EntryNotFound",
                "There is no corresponding enrolment record for that tutorial");

            public static readonly Error StudentAlreadyEnrolled = new(
                "GroupTutorials.TutorialEnrolment.StudentAlreadyEnrolled",
                "The student is already actively enrolled in this tutorial");
        }

        public static class TutorialRoll
        {
            public static readonly Func<DateOnly, Error> RollAlreadyExistsForDate = rollDate => new Error(
                "GroupTutorials.TutorialRoll.RollAlreadyExistsForDate",
                $"A roll for date {rollDate.ToShortDateString()} already exists");

            public static readonly Func<DateOnly, Error> RollDateInvalid = rollDate => new Error(
                "GroupTutorials.TutorialRoll.RollDateInvalid",
                $"Cannot create a roll for {rollDate.ToShortDateString()} as this is not a valid date for this tutorial");

            public static readonly Func<Guid, Error> NotFound = id => new Error(
                "GroupTutorials.TutorialRoll.NotFound",
                $"A roll with the Id {id} could not be found");

            public static readonly Error SubmitInvalidStatus = new Error(
                "GroupTutorials.TutorialRoll.SubmitInvalidStatus",
                "Cannot submit a roll that has been cancelled or previously submitted");

            public static readonly Func<string, Error> StudentNotFound = student => new Error(
                "GroupTutorials.TutorialRoll.StudentNotFound",
                $"Cannot find an attendance record for student with Id {student} attached to the roll");

            public static readonly Func<string, Error> RemoveEnrolledStudent = student => new Error(
                "GroupTutorials.TutorialRoll.RemoveEnrolledStudent",
                $"Cannot remove student with Id {student} from the roll as they are enrolled in the tutorial");
        }

    }

    public static class LinkedSystems
    {
        public static class Teams
        {
            public static readonly Error TeamNotFoundInDatabase = new(
                "LinkedSystems.Teams.TeamNotFoundInDatabase",
                "The Team could not be found in the database");

            public static readonly Func<Guid, Error> AlreadyExists = id => new Error(
                "LinkedSystems.Teams.AlreadyExists",
                $"The Team with Id {id} could not be created because it already exists in the database");
        }
    }

    public static class Partners
    {
        public static class Staff
        {
            public static readonly Func<int, Error> NotFoundLinkedToOffering = id => new Error(
                "Partners.Staff.NotFoundLinkedToOffering",
                $"Could not retrieve list of teachers for the offering {id}");

            public static readonly Func<string, Error> NotFound = id => new Error(
                "Partners.Staff.TeacherNotFound",
                $"A teacher with the Id {id} could not be found");

            public static readonly Func<string, Error> NotFoundByEmail = email => new Error(
                "Partners.Staff.TeacherNotFound",
                $"A teacher with the Email Address {email} could not be found");
        }

        public static class Student
        {
            public static readonly Func<string, Error> NotFound = id => new Error(
                "Partners.Student.NotFound",
                $"A student with the Id {id} could not be found");
        }
    }

    public static class Permissions
    {
        public static readonly Error Unauthorised = new Error(
            "Permissions.Unauthorised",
            "You do not have the required permissions to perform this action");
    }

    public static class ValueObjects
    {
        public static class EmailAddress
        {
            public static readonly Error EmailEmpty = new Error(
                "ValueObjects.EmailAddress.EmailEmpty",
                "Email Address must no tbe empty.");

            public static readonly Error EmailInvalid = new Error(
                "ValueObjects.EmailAddress.EmailInvalid",
                "Email Address is not valid.");
        }

        public static class EmailRecipient
        {
            public static readonly Error NameEmpty = new Error(
                "ValueObjects.EmailRecipient.NameEmpty",
                "Email Recipient must have a valid name.");
        }

        public static class Name
        {
            public static readonly Error FirstNameEmpty = new Error(
                "ValueObjects.Name.FirstNameEmpty",
                "First Name must not be empty.");

            public static readonly Error LastNameEmpty = new Error(
                "ValueObjects.Name.LastNameEmpty",
                "Last Name must not be empty.");
        }
    }
}
