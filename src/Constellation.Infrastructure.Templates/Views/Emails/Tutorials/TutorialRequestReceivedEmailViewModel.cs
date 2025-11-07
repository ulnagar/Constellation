namespace Constellation.Infrastructure.Templates.Views.Emails.Tutorials;

using Constellation.Core.Enums;
using Constellation.Core.Models.Tutorials.Enums;
using Core.ValueObjects;
using Shared;

public sealed class TutorialRequestReceivedEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/Tutorials/TutorialRequestReceivedEmail.cshtml";

    public Name Student { get; set; }
    public Grade Grade { get; set; }
    public string School { get;  set; }
    public string Justification { get;  set; }

    public TutorialType Type { get;  set; }
    public string Subject { get;  set; }
    public string SupportType => Type == TutorialType.Study ? Type.ToString() : $"{Type} - {Subject}";
}
