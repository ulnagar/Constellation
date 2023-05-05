namespace Constellation.Infrastructure.Templates.Views.Emails.Awards;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class NewAwardCertificateEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string AwardType { get; set; }
    public string TeacherName { get; set; }
    public string AwardReason { get; set; }
    public DateTime AwardedOn { get; set; }
}
