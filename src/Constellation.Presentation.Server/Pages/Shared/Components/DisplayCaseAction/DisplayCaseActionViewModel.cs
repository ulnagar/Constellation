namespace Constellation.Presentation.Server.Pages.Shared.Components.DisplayCaseAction;

using Core.Models.WorkFlow;

public class DisplayCaseActionViewModel
{
    public Action Action { get; set; }
    public bool AssignedToMe { get; set; }
    public bool IsAdmin { get; set; }
    public bool ShowMenu { get; set; }
}