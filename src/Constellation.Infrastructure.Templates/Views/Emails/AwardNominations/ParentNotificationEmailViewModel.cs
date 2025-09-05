namespace Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;

using Core.Models.Awards;
using Core.ValueObjects;
using Shared;
using System;
using System.Collections.Generic;

public sealed class ParentNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/AwardNominations/ParentNotificationEmail.cshtml";

    public Name Parent { get; set; }
    public Name Student { get; set; }
    public string School { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public List<Nomination> Awards { get; set; } = [];
}
