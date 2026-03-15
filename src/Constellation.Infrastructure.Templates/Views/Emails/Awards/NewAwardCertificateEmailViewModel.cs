namespace Constellation.Infrastructure.Templates.Views.Emails.Awards;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public sealed class NewAwardCertificateEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Awards/NewAwardCertificateEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string StudentName { get; set; }
    public required string AwardType { get; set; }
    public required string TeacherName { get; set; }
    public required string AwardReason { get; set; }
    public required DateTime AwardedOn { get; set; }
}
