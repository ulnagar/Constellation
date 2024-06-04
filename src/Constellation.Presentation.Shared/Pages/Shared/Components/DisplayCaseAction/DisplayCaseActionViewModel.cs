namespace Constellation.Presentation.Shared.Pages.Shared.Components.DisplayCaseAction;

using Constellation.Core.Models.WorkFlow;

public class DisplayCaseActionViewModel
{
    public Action Action { get; set; }
    public bool AssignedToMe { get; set; }
    public bool IsAdmin { get; set; }
    public bool ShowMenu { get; set; }
}