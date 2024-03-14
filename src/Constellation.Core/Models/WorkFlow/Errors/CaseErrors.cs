namespace Constellation.Core.Models.WorkFlow.Errors;

using Shared;

public static class CaseErrors
{
    public static class CaseDetail
    {
        public static class Create
        {
            public static Error StudentNull = new(
                "Case.Detail.Create.StudentNull",
                "The student cannot be empty or null");

            public static Error AttendanceValueNull = new(
                "Case.Detail.Create.AttendanceValueNull",
                "The Attendance Value cannot be empty or null");

            public static Error StudentMismatch = new(
                "Case.Detail.Create.StudentMismatch",
                "The Attendance Value is not linked to the Student");
        }
    }

    public static class Action
    {
        public static class Assign
        {
            public static Error StaffNull = new(
                "Case.Action.Assign.StaffNull",
                "Cannot assign a Case Action to this Staff Member");

            public static Error StaffDeleted = new(
                "Case.Action.Assign.StaffDeleted",
                "Cannot assign a Case Action to a deleted Staff Member");
        }

        public static class AddNote
        {
            public static Error MessageBlank = new(
                "Case.Action.AddNote.MessageBlank",
                "Cannot create an ActionNote with a blank message");

            public static Error UserNull = new(
                "Case.Action.AddNote.UserNull",
                "Cannot create an ActionNote without a User");
        }

        public static class UpdateStatus
        {
            public static Error StatusNull = new(
                "Case.Action.UpdateStatus.StatusNull",
                "Cannot change Status to blank");
        }
    }
}
