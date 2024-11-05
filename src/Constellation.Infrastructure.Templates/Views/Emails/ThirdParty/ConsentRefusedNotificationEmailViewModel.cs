namespace Constellation.Infrastructure.Templates.Views.Emails.ThirdParty;

using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class ConsentRefusedNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/ThirdParty/ConsentRefusedNotificationEmail.cshtml";

    public string Student { get; set; }
    public DateOnly SubmittedOn { get; set; }
    public List<string> RefusedConsents { get; set; }
}
