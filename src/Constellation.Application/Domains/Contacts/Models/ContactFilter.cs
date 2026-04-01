namespace Constellation.Application.Domains.Contacts.Models;

using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;

public sealed class ContactFilter
{
    public List<OfferingId> OfferingIds { get; set; } = [];
    public List<Grade> Grades { get; set; } = [];
    public List<SchoolCode> SchoolCodes { get; set; } = [];
    public List<ContactCategory> Categories { get; set; } = [];
    public List<StudentFlag> Flags { get; set; } = [];
    public List<CourseId> CourseIds { get; set; } = [];

    public FilterAction Action { get; set; } = FilterAction.Filter;

    public enum FilterAction
    {
        Filter,
        Export,
        Email
    }

    public bool AnyDefined => OfferingIds.Count > 0
                              || Grades.Count > 0
                              || SchoolCodes.Count > 0
                              || Categories.Count > 0
                              || Flags.Count > 0
                              || CourseIds.Count > 0;
}