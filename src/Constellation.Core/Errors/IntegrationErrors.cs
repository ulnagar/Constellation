namespace Constellation.Core.Errors;

using Constellation.Core.Shared;

public static class IntegrationErrors
{
    public static class Absences
    {
        public static class Notifications
        {
            public static class Parents
            {
                public static readonly Error NoAbsencesDetected = new(
                    "Absences.Notifications.Parents.NoAbsencesDetected",
                    "Could not send notification to parents as no absences were selected");
            }
        }
    }

    public static class Exports
    {
        public static readonly Error NoItemsToInclude = new(
            "Export.NoItemsToInclude",
            "Could not find any items to include in report");
    }
}
