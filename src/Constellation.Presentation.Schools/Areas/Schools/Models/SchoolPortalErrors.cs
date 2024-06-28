namespace Constellation.Presentation.Schools.Areas.Schools.Models;

using Core.Shared;

public static class SchoolPortalErrors
{
    public static readonly Error NoSchoolSelected = new(
        "Portal.School.NoSchoolSelected",
        "A valid Partner School has not been selected");
}
