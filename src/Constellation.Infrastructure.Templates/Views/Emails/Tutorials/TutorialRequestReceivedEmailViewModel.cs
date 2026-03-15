namespace Constellation.Infrastructure.Templates.Views.Emails.Tutorials;

using Constellation.Core.Enums;
using Constellation.Core.Models.Tutorials.Enums;
using Core.ValueObjects;
using Shared;

public sealed class TutorialRequestReceivedEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Tutorials/TutorialRequestReceivedEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required Name Student { get; set; }
    public required Grade Grade { get; set; }
    public required string School { get;  set; }
    public required string Justification { get;  set; }

    public required TutorialType Type { get;  set; }
    public required string Subject { get;  set; }
    public string SupportType => Type == TutorialType.Study ? Type.ToString() : $"{Type} - {Subject}";
}
