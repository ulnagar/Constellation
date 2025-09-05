namespace Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;

using Constellation.Core.Models.Awards;
using Core.ValueObjects;
using Shared;
using System;
using System.Collections.Generic;

public sealed class SchoolNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/AwardNominations/SchoolNotificationEmail.cshtml";

    public Name Contact { get; set; }
    public string School { get; set; }
    public DateOnly DeliveryDate { get; set; }
    
    public Dictionary<Name, List<Nomination>> Students { get; set; }
}
