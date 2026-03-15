namespace Constellation.Infrastructure.Templates.Views.Emails.ThirdParty;

using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class ConsentRefusedNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/ThirdParty/ConsentRefusedNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string Student { get; set; }
    public required DateOnly SubmittedOn { get; set; }
    public List<string> RefusedConsents { get; set; } = [];
}
