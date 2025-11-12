namespace Constellation.Infrastructure.Templates.Views.Emails.Tutorials;

using Constellation.Core.Enums;
using Constellation.Core.Models.Tutorials.Enums;
using Core.Models.LinkedSystems;
using Core.ValueObjects;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class TutorialRequestScheduledEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/Tutorials/TutorialRequestScheduledEmail.cshtml";

    public Name Student { get; set; }
    public Grade Grade { get; set; }
    public string School { get; set; }

    public TutorialType Type { get; set; }
    public string Subject { get; set; }
    public string SupportType => Type == TutorialType.Study ? Type.ToString() : $"{Type} - {Subject}";

    public List<(string Period, string Teacher)> ScheduledPeriods { get; set; } = new();
    public Team TutorialTeam { get; set; }
    public DateOnly StartDate { get; set; }
}
