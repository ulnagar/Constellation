namespace Constellation.Core.Models.Enrolment.Errors;

using Constellation.Core.Shared;

public static class EnrolmentErrors
{
    public static Error AlreadyDeleted => new(
        "Enrolment.Enrolment",
        "This enrolment is already marked deleted");
}
