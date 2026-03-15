namespace Constellation.Infrastructure.Templates.Views.Emails.AwardNominations;

using Core.Models.Awards;
using Core.ValueObjects;
using Shared;
using System;
using System.Collections.Generic;
using System.Globalization;

public sealed class ParentNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/AwardNominations/ParentNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required Name Parent { get; set; }
    public required Name Student { get; set; }
    public required string School { get; set; }
    public required DateOnly DeliveryDate { get; set; }
    public List<Nomination> Awards { get; set; } = [];
    public string Year => DeliveryDate.Year.ToString(CultureInfo.InvariantCulture);
}
