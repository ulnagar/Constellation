namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.AppSettings;
using Core.Models.StaffMembers;
using Core.ValueObjects;

public sealed record LessonsConfiguration
{
    public LessonsConfiguration(
        LessonsSettings settings,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        CoordinatorEmail = settings.CoordinatorEmail;
        CoordinatorName = settings.CoordinatorName;
        CoordinatorTitle = settings.CoordinatorTitle;
        Contacts = contacts;
    }

    public LessonsConfiguration(
        string name,
        string title,
        string email,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        CoordinatorEmail = email;
        CoordinatorName = name;
        CoordinatorTitle = title;
        Contacts = contacts;
    }

    public string CoordinatorEmail { get; init; }
    public string CoordinatorName { get; init; }
    public string CoordinatorTitle { get; init; }

    public IReadOnlyDictionary<StaffMember, List<Grade>> Contacts { get; }

    public EmailRecipient Recipient => EmailRecipient.Create(CoordinatorName, CoordinatorEmail).IsSuccess
        ? EmailRecipient.Create(CoordinatorName, CoordinatorEmail).Value
        : EmailRecipient.AuroraCollege;
}