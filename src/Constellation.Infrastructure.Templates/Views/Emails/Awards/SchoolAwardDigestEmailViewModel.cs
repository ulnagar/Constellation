namespace Constellation.Infrastructure.Templates.Views.Emails.Awards;

using Application.Domains.MeritAwards.Awards.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class SchoolAwardDigestEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Awards/SchoolAwardDigestEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public readonly string Link = $"{BaseUrl}";
    public required List<StudentAwardTally> Students { get; init; }
}
