namespace Constellation.Infrastructure.Templates.Views.Emails.WorkFlow;

using Core.Models.WorkFlow.Identifiers;
using Shared;

public sealed class ActionCancelledEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/WorkFlow/ActionCancelledEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public string Link => $"{BaseUrl}{LinkPart}";

    public required string TeacherName { get; set; }
    public required CaseId CaseId { get; set; }
    public required string CaseDescription { get; set; }
    public required ActionId ActionId { get; set; }
    public required string ActionDescription { get; set; }

    public required string LinkPart { get; set; }
}
