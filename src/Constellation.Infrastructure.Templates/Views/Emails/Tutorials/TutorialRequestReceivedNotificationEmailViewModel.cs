namespace Constellation.Infrastructure.Templates.Views.Emails.Tutorials;

using Constellation.Core.Enums;
using Constellation.Core.Models.Tutorials.Enums;
using Constellation.Core.Models.Tutorials.Identifiers;
using Core.ValueObjects;
using Shared;

public sealed class TutorialRequestReceivedNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/Tutorials/TutorialRequestReceivedNotificationEmail.cshtml";

    public Name Student { get; set; }
    public Grade Grade { get; set; }
    public string School { get; set; }
    public string Justification { get; set; }

    public TutorialType Type { get; set; }
    public string Subject { get; set; }
    public string SupportType => Type == TutorialType.Study ? Type.ToString() : $"{Type} - {Subject}";

    public RequestId RequestId { get; set; }
    public string ApprovalLink => $"https://acos.aurora.nsw.edu.au/Staff/Subject/Tutorials/Requests/Details/{RequestId}";
}
