namespace Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;

using Constellation.Core.Models.Awards;
using Core.ValueObjects;
using Shared;
using System;
using System.Collections.Generic;

public sealed class SchoolNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/AwardNominations/SchoolNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required Name Contact { get; set; }
    public required string School { get; set; }
    public required DateOnly DeliveryDate { get; set; }

    public Dictionary<Name, List<Nomination>> Students { get; set; } = [];
}
