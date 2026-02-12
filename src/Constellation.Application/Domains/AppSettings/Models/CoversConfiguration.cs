namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.AppSettings;
using Core.Models.StaffMembers;
using System.Collections.Generic;

public sealed record CoversConfiguration
{
    public CoversConfiguration(
        CoversSettings settings,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        ContactName = settings.ContactName;
        ContactTitle = settings.ContactTitle;
        ContactPhone = settings.ContactPhone;
        Contacts = contacts;
    }

    public CoversConfiguration(
        string name,
        string title,
        string phone,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        ContactName = name;
        ContactTitle = title;
        ContactPhone = phone;
        Contacts = contacts;
    }

    public string ContactName { get; init; }
    public string ContactTitle { get; init; }
    public string ContactPhone { get; init; }

    public IReadOnlyDictionary<StaffMember, List<Grade>> Contacts { get; }
}