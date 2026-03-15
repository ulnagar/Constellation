namespace Constellation.Infrastructure.Templates.Views.Emails.Tutorials;

using Constellation.Core.Enums;
using Constellation.Core.Models.Tutorials.Enums;
using Core.ValueObjects;
using Shared;
using System;
using System.Collections.Generic;

public sealed class TutorialRequestScheduledEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Tutorials/TutorialRequestScheduledEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required Name Student { get; set; }
    public required Grade Grade { get; set; }
    public required string School { get; set; }

    public required TutorialType Type { get; set; }
    public required string Subject { get; set; }
    public string SupportType => Type == TutorialType.Study ? Type.ToString() : $"{Type} - {Subject}";

    public List<(string Period, string Teacher)> ScheduledPeriods { get; set; } = [];
    public required string TutorialTeam { get; set; }
    public required DateOnly StartDate { get; set; }
}
